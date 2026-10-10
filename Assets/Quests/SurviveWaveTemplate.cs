using Monsters;
using Spawning;
using System;
using UnityEngine;

namespace Quests
{
    [Serializable]
    public class SurviveWaveTemplate : QuestTemplate
    {
        public MonsterSO monster;
        public ScalingValue monsterCount = new ScalingValue(40f, 8f);
        public ScalingValue windowSeconds = new ScalingValue(30f, 1f);

        [Range(0f, 0.95f)]
        [Tooltip("Fail if the hero drops below this fraction of max health during the wave. 0 = ignore.")]
        public float minHeroHealthFraction = 0.35f;

        public Vector2 spawnDistance = new Vector2(6f, 16f);
        public Vector2 startWindow = new Vector2(0.2f, 0.7f);

        public override bool CanCreate(QuestContext ctx) => base.CanCreate(ctx) && monster != null;

        public override Quest Create(QuestContext ctx)
        {
            float window = Mathf.Max(5f, windowSeconds.Evaluate(ctx.Difficulty));
            float start = Mathf.Lerp(startWindow.x, startWindow.y, (float)ctx.Random.NextDouble()) * ctx.Plan.Duration;
            start = Mathf.Min(start, Mathf.Max(0f, ctx.Plan.Duration - window)); // must finish inside the level timer

            return new SurviveWaveQuest(monster, Mathf.Max(1, monsterCount.EvaluateInt(ctx.Difficulty)),
                start, window, minHeroHealthFraction, spawnDistance);
        }
    }
}