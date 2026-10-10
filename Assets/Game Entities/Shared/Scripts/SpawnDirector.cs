using System;
using System.Collections.Generic;
using System.Linq;
using GameManagerCalls;
using GlobalMap;
using Monsters;
using Quests;
using Units;
using UnityEngine;
using Spawning;
namespace Spawning
{
    /// <summary>
    /// Replaces the tile timeline. Builds a LevelPlan when the local map is created (so the UI can show
    /// the required unit count and quests before play), then runs it once the level is played:
    /// a level timer, grouped allied-unit spawning, scheduled monster waves and monster pacing.
    /// </summary>
    public class SpawnDirector : Singleton<SpawnDirector>,
        IOnAfterCreateLocalMap, IOnPlayLocalLevel, IOnCompleteLocalMapLevel, IOnGameEnd, IOnClear
    {
        public SpawnDirectorSettings Settings { get; private set; }
        public LevelPlan Plan { get; private set; }
        public float Difficulty { get; private set; }

        public bool IsRunning { get; private set; }
        public bool SpawnPhaseFinished { get; private set; }
        public float Elapsed { get; private set; }
        public float CurrentPacing { get; private set; } = 1f;
        public float TimeRemaining => Plan == null ? 0f : Mathf.Max(0f, Plan.Duration - Elapsed);

        public event Action<LevelPlan> onPlanCreated;
        public event Action onLevelStarted;
        public event Action onSpawnPhaseFinished;
        public event Action<ScheduledWave> onWaveStarted;

        private System.Random rng;
        private readonly Queue<UnitSO> pendingGroup = new Queue<UnitSO>();
        private int unitsQueued;
        private float nextUnitTime;
        private int nextWave;

        // ---------------------------------------------------------
        //  Level lifecycle
        // ---------------------------------------------------------

        public void AfterCreateLocalMap()
        {
            Stop();
            Plan = null;

            GlobalMapPoint point = GlobalMapGenerator.Instance.CurrentMapPoint;
            Settings = point.GenerationSettings != null ? point.GenerationSettings.SpawnDirectorSettings : null;

            if (Settings == null)
            {
                Debug.LogWarning("SpawnDirector: current biome has no SpawnDirectorSettings.");
                return;
            }

            Difficulty = DifficultyLevelManager.Instance.DifficultyLevel;
            rng = new System.Random(point.Seed);

            Plan = LevelPlan.Generate(Settings, point.GenerationSettings.MonsterSpawnSettings, Difficulty, rng);

            if (QuestManager.Instance != null)
                QuestManager.Instance.GenerateQuests(Settings.questSettings, Difficulty, rng, Plan);

            onPlanCreated?.Invoke(Plan);
        }

        public void OnPlayLocalLevel()
        {
            if (Plan == null)
                return;

            Elapsed = 0f;
            nextWave = 0;
            unitsQueued = 0;
            nextUnitTime = 0f;
            pendingGroup.Clear();
            SpawnPhaseFinished = false;
            IsRunning = true;

            MonsterSpawnManager.Instance.ConfigureNormalSpawns(
                Plan.ActiveNormalSpawns, Plan.MaxMonstersAlive, Settings.chaos);

            onLevelStarted?.Invoke();
        }

        public void OnCompleteLocalMapLevel() => Stop();
        public void OnGameEnd(string message) => Stop();
        public void Clear()
        {
            Stop();
            Plan = null;
        }

        private void Stop()
        {
            IsRunning = false;
            SpawnPhaseFinished = false;
            Elapsed = 0f;
            pendingGroup.Clear();
        }

        private void FinishSpawnPhase()
        {
            IsRunning = false;
            SpawnPhaseFinished = true;
            MonsterSpawnManager.Instance.SetSpawnIntensity(0f);
            onSpawnPhaseFinished?.Invoke();
        }

        // ---------------------------------------------------------
        //  Main loop
        // ---------------------------------------------------------

        private void Update()
        {
            if (!IsRunning || Plan == null)
                return;

            Elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(Elapsed / Plan.Duration);
            CurrentPacing = Mathf.Max(0f, Settings.pacing.Evaluate(t));

            MonsterSpawnManager.Instance.SetSpawnIntensity(CurrentPacing * Plan.MonsterDensity);

            TickWaves();
            TickUnits();

            if (Elapsed >= Plan.Duration)
                FinishSpawnPhase();
        }

        private void TickWaves()
        {
            while (nextWave < Plan.Waves.Count && Elapsed >= Plan.Waves[nextWave].StartTime)
            {
                ScheduledWave wave = Plan.Waves[nextWave++];
                MonsterSpawnManager.Instance.BeginSpawningWave(wave.Spawn, wave.Multiplier, Difficulty);
                onWaveStarted?.Invoke(wave);
            }
        }

        // ---------------------------------------------------------
        //  Allied units
        // ---------------------------------------------------------

        private void TickUnits()
        {
            if (Elapsed < nextUnitTime)
                return;

            if (pendingGroup.Count == 0)
            {
                if (unitsQueued >= Plan.TotalUnits)
                    return;

                QueueNextGroup();
            }

            if (pendingGroup.Count == 0)
                return;

            UnitSO unit = pendingGroup.Dequeue();
            UnitSpawningManager.Instance.SpawnUnit(unit);

            nextUnitTime = Elapsed + (pendingGroup.Count > 0 ? Settings.groupStagger : DelayToNextGroup());
        }

        private void QueueNextGroup()
        {
            UnitSO unit = PickRandomUnit();
            if (unit == null)
            {
                unitsQueued = Plan.TotalUnits; // nothing eligible to spawn
                return;
            }

            int remaining = Plan.TotalUnits - unitsQueued;
            int size = Mathf.Min(remaining, rng.Next(Plan.GroupMin, Plan.GroupMax + 1));

            for (int i = 0; i < size; i++)
                pendingGroup.Enqueue(unit);

            unitsQueued += size;
        }

        /// <summary>
        /// Re-derived after every group from what is left, so the plan stays on schedule however
        /// much the jitter and pacing curve push individual gaps around.
        /// </summary>
        private float DelayToNextGroup()
        {
            int unitsLeft = Plan.TotalUnits - unitsQueued;
            if (unitsLeft <= 0)
                return float.MaxValue;

            float window = Plan.UnitWindowEnd - Elapsed;
            if (window <= 0f)
                return Settings.groupStagger; // behind schedule: flush the rest

            float averageGroup = (Plan.GroupMin + Plan.GroupMax) * 0.5f;
            float groupsLeft = Mathf.Max(1f, Mathf.Ceil(unitsLeft / averageGroup));

            float delay = window / groupsLeft;
            delay *= SpawnRandom.Jitter(rng, Settings.chaos) / Mathf.Max(0.25f, CurrentPacing);

            return Mathf.Max(Settings.groupStagger, delay);
        }

        public UnitSO PickRandomUnit()
        {
            if (Settings == null)
                return null;

            List<UnitSpawnEntry> eligible = Settings.unitPool
                .Where(e => e != null && e.unit != null && e.minDifficulty <= Difficulty)
                .ToList();

            if (eligible.Count == 0)
                return null;

            return SpawnRandom.PickWeighted(eligible, e => e.weight, rng).unit;
        }
    }
}
