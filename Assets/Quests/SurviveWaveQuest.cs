using Heros;
using Monsters;
using UnityEngine;

namespace Quests
{
    // =====================================================================
    //  Survive a large wave
    // =====================================================================

    public class SurviveWaveQuest : Quest
    {
        private readonly MonsterSO monster;
        private readonly int count;
        private readonly float startAt;
        private readonly float window;
        private readonly float minHeroHealthFraction;
        private readonly Vector2 spawnDistance;

        private bool started;

        public SurviveWaveQuest(MonsterSO monster, int count, float startAt, float window, float minHeroHealthFraction, Vector2 spawnDistance)
        {
            this.monster = monster;
            this.count = count;
            this.startAt = startAt;
            this.window = window;
            this.minHeroHealthFraction = minHeroHealthFraction;
            this.spawnDistance = spawnDistance;
        }

        public override string Title => "Hold the Line";

        public override string Description => minHeroHealthFraction > 0f
            ? $"Survive a swarm of {count} {monster.Title} for {window:0}s without dropping below {minHeroHealthFraction:P0} health."
            : $"Survive a swarm of {count} {monster.Title} for {window:0}s.";

        public override string ProgressText
            => started ? $"{Mathf.Max(0f, startAt + window - Context.Director.Elapsed):0}s left" : "Incoming...";

        public override void Tick(float deltaTime)
        {
            float t = Context.Director.Elapsed;

            if (!started)
            {
                if (t < startAt)
                    return;

                started = true;
                MonsterSpawnManager.Instance.SpawnQuestWave(
                    monster, count, window * 0.5f, spawnDistance.x, spawnDistance.y, Context.Difficulty);
                NotifyChanged();
            }

            if (minHeroHealthFraction > 0f && HeroManager.Instance.ActiveHero != null)
            {
                Health health = HeroManager.Instance.ActiveHero.HealthAndShieldManager.Health;
                if (health.MaxHealth > 0 && (float)health.CurrentHealth / health.MaxHealth < minHeroHealthFraction)
                {
                    Fail();
                    return;
                }
            }

            if (t >= startAt + window)
                Complete();
        }
    }
}