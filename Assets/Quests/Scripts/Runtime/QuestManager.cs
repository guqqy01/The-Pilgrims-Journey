using GameManagerCalls;
using GlobalMap;
using Monsters;
using Spawning;
using System;
using System.Collections.Generic;
using System.Linq;
using Units;
using UnityEngine;

namespace Quests
{
    public class QuestManager : Singleton<QuestManager>, IOnClear, IOnGameEnd
    {
        private readonly List<Quest> _mainQuests = new List<Quest>();
        public IReadOnlyList<Quest> MainQuests => _mainQuests;

        private readonly List<Quest> _optionalQuests = new List<Quest>();
        public IReadOnlyList<Quest> OptionalQuests => _optionalQuests;

        public event Action OnQuestsGenerated;
        public event Action<Quest> OnQuestChanged;
        public event Action<Quest> OnQuestRewarded;

        public event Action<Quest> OnMainQuestFailed;
        public event Action OnAllMainQuestsCompleted;

        private bool mainResolved;
        private Quest pendingFailedQuest;
        private bool pendingAllCompleted;

        private bool active;

        private void Start()
        {
            UnitSpawningManager.WhenReady(m =>
            {
                m.onUnitSpawned += HandleUnitSpawned;
                m.onUnitDied += HandleUnitDied;
                m.onUnitEscaped += HandleUnitEscaped;
            });
            MonsterSpawnManager.WhenReady(m => m.OnMonsterKilled += HandleMonsterKilled);
            MonsterSpawnManager.WhenReady(m => m.OnBossKilled += HandleBossKilled);
            SpawnDirector.WhenReady(d => d.onLevelStarted += BeginQuests);
        }

        protected override void CleanUp()
        {
            base.CleanUp();
            UnitSpawningManager.IfReady(m =>
            {
                m.onUnitSpawned -= HandleUnitSpawned;
                m.onUnitDied -= HandleUnitDied;
                m.onUnitEscaped -= HandleUnitEscaped;
            });
            MonsterSpawnManager.IfReady(m => m.OnMonsterKilled -= HandleMonsterKilled);
            MonsterSpawnManager.IfReady(m => m.OnBossKilled -= HandleBossKilled);
            SpawnDirector.IfReady(d => d.onLevelStarted -= BeginQuests);
        }

        public void GenerateQuests(QuestSettings settings, float difficulty, System.Random rng, LevelPlan plan)
        {
            ClearQuests();

            var context = new QuestContext(difficulty, plan, rng);

            CreateMainQuests(context);

            if (settings != null && settings.templates != null)
                CreateOptionalQuests(settings, context);

            OnQuestsGenerated?.Invoke();
        }

        private void CreateMainQuests(QuestContext context)
        {
            AddMainQuest(new SafePassageQuest(context.Plan.RequiredUnits), context);

            bool isBossLevel = GlobalMapGenerator.Instance.CurrentMapPoint.Type == GlobalMapPointType.Relic;
            if (isBossLevel)
                AddMainQuest(new BossKilledQuest(), context);
        }

        private void AddMainQuest(Quest quest, QuestContext context)
        {
            AddQuest(_mainQuests, quest, context);
            quest.onChanged += HandleMainQuestChanged;
        }

        private void HandleMainQuestChanged(Quest quest)
        {
            if (mainResolved)
                return;

            if (quest.State == QuestState.Failed)
            {
                mainResolved = true;
                pendingFailedQuest = quest;
            }
            else if (_mainQuests.Count > 0 && AllMainQuestsCompleted)
            {
                mainResolved = true;
                pendingAllCompleted = true;
            }
        }

        private void CreateOptionalQuests(QuestSettings settings, QuestContext context)
        {
            float difficulty = context.Difficulty;
            List<QuestTemplate> pool = settings.templates.Where(t => t != null && t.CanCreate(context)).ToList();
            int count = Mathf.Min(settings.questCount.EvaluateInt(difficulty), pool.Count);

            for (int i = 0; i < count; i++)
            {
                QuestTemplate template = SpawnRandom.PickWeighted(pool, t => t.weight, context.Random);
                pool.Remove(template);

                Quest quest = template.Create(context);
                if (quest == null)
                    continue;

                quest.Reward = template.reward.Roll(difficulty);
                AddQuest(_optionalQuests, quest, context);
            }
        }
        private void AddQuest(List<Quest> list, Quest quest, QuestContext context)
        {
            quest.Init(context);
            quest.onChanged += q => OnQuestChanged?.Invoke(q);
            list.Add(quest);
        }

        private void BeginQuests()
        {
            active = true;
            foreach (Quest quest in _mainQuests) quest.Begin();
            foreach (Quest quest in _optionalQuests) quest.Begin();
        }

        private void Update()
        {
            DispatchMainQuestResult();

            if (!active)
                return;

            float dt = Time.deltaTime;
            TickList(_mainQuests, dt);
            TickList(_optionalQuests, dt);
        }

        private void DispatchMainQuestResult()
        {
            if (pendingFailedQuest != null)
            {
                Quest failed = pendingFailedQuest;
                pendingFailedQuest = null;
                OnMainQuestFailed?.Invoke(failed);
                return;
            }

            if (pendingAllCompleted)
            {
                pendingAllCompleted = false;
                OnAllMainQuestsCompleted?.Invoke();
            }
        }

        private static void TickList(List<Quest> list, float dt)
        {
            foreach (Quest quest in list)
            {
                if (quest.State == QuestState.Active)
                    quest.Tick(dt);
            }
        }


        /// <summary>Resolves anything still active. Call when the level ends.</summary>
        public void FinalizeQuests()
        {
            if (!active)
                return;

            active = false;

            // The level is over: whatever happens to quests now must not trigger a game over.
            mainResolved = true;
            pendingFailedQuest = null;
            pendingAllCompleted = false;

            ForEachInList(_mainQuests, q => q.OnLevelEnd());
            ForEachInList(_optionalQuests, q => q.OnLevelEnd());
        }

        /// <summary>
        /// Announces the reward of every completed quest. I haven't seen RewardManager, so this raises
        /// onQuestRewarded for you to hook up rather than guessing at its API.
        /// </summary>
        public void GrantRewards()
        {
            foreach (Quest quest in _optionalQuests)
            {
                if (quest.State != QuestState.Completed)
                    continue;

                RewardManager.Instance?.GiveReward(quest.Reward.Type, quest.Reward.Amount);
                OnQuestRewarded?.Invoke(quest);
            }
        }

        public bool AllMainQuestsCompleted => _mainQuests.All(q => q.State == QuestState.Completed);
        public void NotifyUnitPassed(int totalPassed) => ForEachActive(q => q.OnUnitPassed(totalPassed));
        private void HandleUnitSpawned(Unit unit) => ForEachActive(q => q.OnUnitSpawned(unit));
        private void HandleUnitDied(Unit unit) => ForEachActive(q => q.OnUnitDied(unit));
        private void HandleUnitEscaped(Unit unit) => ForEachActive(q => q.OnUnitEscaped(unit));
        private void HandleMonsterKilled(Monster monster) => ForEachActive(q => q.OnMonsterKilled(monster));
        private void HandleBossKilled(Monster monster) => ForEachActive(q => q.OnBossKilled(monster));
        private void ForEachActive(Action<Quest> action)
        {
            if (!active)
                return;

            ForEachInList(_mainQuests, action);
            ForEachInList(_optionalQuests, action);
        }

        private static void ForEachInList(List<Quest> list, Action<Quest> action)
        {
            foreach (Quest quest in list)
            {
                if (quest.State == QuestState.Active)
                    action(quest);
            }
        }

        private void ClearQuests()
        {
            _mainQuests.Clear();
            _optionalQuests.Clear();
            active = false;
            mainResolved = false;
            pendingFailedQuest = null;
            pendingAllCompleted = false;
        }

        public void Clear() => ClearQuests();

        public void OnGameEnd(string message) => active = false;
    }
}