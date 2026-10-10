using GameManagerCalls;
using Monsters;
using Spawning;
using System;
using System.Collections.Generic;
using System.Linq;
using Units;
using UnityEngine;

namespace Quests
{
    public class QuestManager : Singleton<QuestManager>, IOnClear
    {
        private readonly List<Quest> quests = new List<Quest>();
        public IReadOnlyList<Quest> Quests => quests;

        public event Action onQuestsGenerated;
        public event Action<Quest> onQuestChanged;
        public event Action<Quest> onQuestRewarded;

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
            SpawnDirector.IfReady(d => d.onLevelStarted -= BeginQuests);
        }

        public void GenerateQuests(QuestSettings settings, float difficulty, System.Random rng, LevelPlan plan)
        {
            ClearQuests();

            if (settings == null || settings.templates == null)
                return;

            var context = new QuestContext(difficulty, plan, rng);
            List<QuestTemplate> pool = settings.templates.Where(t => t != null && t.CanCreate(context)).ToList();
            int count = Mathf.Min(settings.questCount.EvaluateInt(difficulty), pool.Count);

            for (int i = 0; i < count; i++)
            {
                QuestTemplate template = SpawnRandom.PickWeighted(pool, t => t.weight, rng);
                pool.Remove(template);

                Quest quest = template.Create(context);
                if (quest == null)
                    continue;

                quest.Init(context);
                quest.Reward = template.reward.Roll(difficulty);
                quest.onChanged += q => onQuestChanged?.Invoke(q);
                quests.Add(quest);
            }

            onQuestsGenerated?.Invoke();
        }

        private void BeginQuests()
        {
            active = true;
            foreach (Quest quest in quests)
                quest.Begin();
        }

        private void Update()
        {
            if (!active)
                return;

            foreach (Quest quest in quests)
            {
                if (quest.State == QuestState.Active)
                    quest.Tick(Time.deltaTime);
            }
        }

        /// <summary>Resolves anything still active. Call when the level ends.</summary>
        public void FinalizeQuests()
        {
            if (!active)
                return;

            active = false;

            foreach (Quest quest in quests)
            {
                if (quest.State == QuestState.Active)
                    quest.OnLevelEnd();
            }
        }

        /// <summary>
        /// Announces the reward of every completed quest. I haven't seen RewardManager, so this raises
        /// onQuestRewarded for you to hook up rather than guessing at its API.
        /// </summary>
        public void GrantRewards()
        {
            foreach (Quest quest in quests)
            {
                if (quest.State != QuestState.Completed)
                    continue;

                RewardManager.Instance?.GiveReward(quest.Reward.Type, quest.Reward.Amount);
                onQuestRewarded?.Invoke(quest); // still raised, so the UI can show what was earned
            }
        }

        private void HandleUnitSpawned(Unit unit) => ForEachActive(q => q.OnUnitSpawned(unit));
        private void HandleUnitDied(Unit unit) => ForEachActive(q => q.OnUnitDied(unit));
        private void HandleUnitEscaped(Unit unit) => ForEachActive(q => q.OnUnitEscaped(unit));
        private void HandleMonsterKilled(Monster monster) => ForEachActive(q => q.OnMonsterKilled(monster));

        private void ForEachActive(Action<Quest> action)
        {
            if (!active)
                return;

            foreach (Quest quest in quests)
            {
                if (quest.State == QuestState.Active)
                    action(quest);
            }
        }

        private void ClearQuests()
        {
            quests.Clear();
            active = false;
        }

        public void Clear() => ClearQuests();
    }
}