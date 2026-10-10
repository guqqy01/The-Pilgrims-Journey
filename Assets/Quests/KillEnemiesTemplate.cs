using Monsters;
using Spawning;
using System;
using UnityEngine;

namespace Quests
{
    [Serializable]
    public class KillEnemiesTemplate : QuestTemplate
    {
        public ScalingValue killCount = new ScalingValue(25f, 3f);
        [Tooltip("Leave empty to count any monster.")]
        public MonsterSO specificMonster;

        public override Quest Create(QuestContext ctx)
            => new KillEnemiesQuest(Mathf.Max(1, killCount.EvaluateInt(ctx.Difficulty)), specificMonster);
    }
}