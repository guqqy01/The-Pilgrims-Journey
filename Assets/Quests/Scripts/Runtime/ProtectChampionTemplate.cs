using Spawning;
using System;
using Units;
using UnityEngine;

namespace Quests
{
    [Serializable]
    public class ProtectChampionTemplate : QuestTemplate
    {
        [Tooltip("Leave empty to use a random unit from the level's unit pool.")]
        public UnitSO unitOverride;

        [Tooltip("Percentage bonus to health and shield. 100 = double.")]
        public ScalingValue buffPercent = new ScalingValue(150f, 25f);

        public float scale = 1.3f;

        [Tooltip("The champion arrives somewhere between these fractions of the level.")]
        public Vector2 arrivalWindow = new Vector2(0.1f, 0.5f);

        public override bool CanCreate(QuestContext ctx)
            => base.CanCreate(ctx) && (unitOverride != null || ctx.Director.PickRandomUnit() != null);

        public override Quest Create(QuestContext ctx)
        {
            UnitSO unit = unitOverride != null ? unitOverride : ctx.Director.PickRandomUnit();
            float t = Mathf.Lerp(arrivalWindow.x, arrivalWindow.y, (float)ctx.Random.NextDouble());

            return new ProtectChampionQuest(unit, buffPercent.Evaluate(ctx.Difficulty), scale, t * ctx.Plan.Duration);
        }
    }
}