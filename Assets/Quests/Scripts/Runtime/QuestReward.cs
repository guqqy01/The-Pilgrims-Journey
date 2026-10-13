namespace Quests
{
    public readonly struct QuestReward
    {
        public readonly Reward Type;
        public readonly int Amount;
        public QuestReward(Reward type, int amount) { Type = type; Amount = amount; }
    }
}