using System.Collections.Generic;
using System.Linq;
using Monsters;
using UnityEngine;

namespace Spawning
{
    /// <summary>The concrete, already-randomised plan for one level. Built once from settings + difficulty + seed.</summary>
    public class LevelPlan
    {
        public float Duration { get; private set; }
        public float UnitWindowEnd { get; private set; }

        public int TotalUnits { get; private set; }
        public int RequiredUnits { get; private set; }
        public int GroupMin { get; private set; }
        public int GroupMax { get; private set; }

        public float MonsterDensity { get; private set; }
        public int MaxMonstersAlive { get; private set; }

        public List<NormalMonsterSpawn> ActiveNormalSpawns { get; private set; } = new List<NormalMonsterSpawn>();
        public List<ScheduledWave> Waves { get; } = new List<ScheduledWave>();

        public static LevelPlan Generate(SpawnDirectorSettings s, MonsterSpawnSettings monsters, float difficulty, System.Random rng)
        {
            var plan = new LevelPlan
            {
                Duration = Mathf.Max(10f, s.levelDuration.Evaluate(difficulty)),
                MonsterDensity = Mathf.Max(0f, s.monsterDensity.Evaluate(difficulty)),
                MaxMonstersAlive = Mathf.Max(1, s.maxMonstersAlive.EvaluateInt(difficulty)),
                TotalUnits = Mathf.Max(1, s.totalUnits.EvaluateInt(difficulty)),
            };

            plan.UnitWindowEnd = plan.Duration * Mathf.Clamp01(s.unitSpawnWindow);

            float fraction = Mathf.Clamp01(s.requiredFraction.Evaluate(difficulty));
            plan.RequiredUnits = Mathf.Clamp(Mathf.CeilToInt(plan.TotalUnits * fraction), 1, plan.TotalUnits);

            plan.GroupMin = Mathf.Max(1, s.groupSizeMin.EvaluateInt(difficulty));
            plan.GroupMax = Mathf.Max(plan.GroupMin, s.groupSizeMax.EvaluateInt(difficulty));

            BuildMonsterRoster(plan, s, monsters, difficulty, rng);
            BuildWaves(plan, s, monsters, difficulty, rng);

            return plan;
        }

        private static void BuildMonsterRoster(LevelPlan plan, SpawnDirectorSettings s, MonsterSpawnSettings monsters, float difficulty, System.Random rng)
        {
            List<NormalMonsterSpawn> normals = monsters?.normalMonsterSpawns?
                .Where(x => x != null && x.monster != null)
                .ToList() ?? new List<NormalMonsterSpawn>();

            SpawnRandom.Shuffle(normals, rng);

            int take = s.activeNormalTypes.EvaluateInt(difficulty);
            if (take > 0 && take < normals.Count)
                normals = normals.Take(take).ToList();

            plan.ActiveNormalSpawns = normals;
        }

        private static void BuildWaves(LevelPlan plan, SpawnDirectorSettings s, MonsterSpawnSettings monsters, float difficulty, System.Random rng)
        {
            List<WaveMonsterSpawn> pool = monsters?.waveMonsterSpawns?
                .Where(x => x != null && x.monster != null)
                .ToList();

            if (pool == null || pool.Count == 0)
                return;

            int count = Mathf.Max(0, s.waveCount.EvaluateInt(difficulty));
            if (count == 0)
                return;

            float lo = Mathf.Clamp01(s.waveWindow.x);
            float hi = Mathf.Max(lo, Mathf.Clamp01(s.waveWindow.y));
            float slot = (hi - lo) / count;

            for (int i = 0; i < count; i++)
            {
                // One wave per slot. chaos moves it around inside its slot, 0 = dead centre.
                float centre = lo + slot * (i + 0.5f);
                float offset = ((float)rng.NextDouble() - 0.5f) * slot * s.chaos;

                WaveMonsterSpawn spawn = pool[rng.Next(pool.Count)];
                int multiplier = Mathf.Max(1, s.waveSizeMultiplier.EvaluateInt(difficulty));

                plan.Waves.Add(new ScheduledWave((centre + offset) * plan.Duration, spawn, multiplier));
            }
        }
    }
}