using System;
using Spawning;

namespace Quests
{
    [Serializable]
    public struct QuestRewardSettings
    {
        public Reward type;
        public ScalingValue amount;

        public QuestReward Roll(float difficulty) => new QuestReward(type, amount.EvaluateInt(difficulty));
    }
}