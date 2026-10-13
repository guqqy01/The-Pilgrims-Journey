using Spawning;
using System;
using UnityEngine;
namespace Quests
{
    [Serializable]
    public class CasualtyLimitTemplate : QuestTemplate
    {
        [Tooltip("Allowed casualties as a fraction of the level's total units.")]
        public ScalingValue allowedFraction = new ScalingValue(0.3f, -0.01f) { clamp = true, min = 0.05f, max = 0.6f };

        public override Quest Create(QuestContext ctx)
        {
            int allowed = Mathf.RoundToInt(ctx.Plan.TotalUnits * allowedFraction.Evaluate(ctx.Difficulty));
            return new CasualtyLimitQuest(Mathf.Max(0, allowed));
        }
    }
}
