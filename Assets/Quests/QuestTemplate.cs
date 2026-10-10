using System;

namespace Quests
{
    /// <summary>Authored (serialized) description of a quest type. Creates a Quest with this level's numbers.</summary>
    [Serializable]
    public abstract class QuestTemplate
    {
        public float weight = 1f;
        public float minDifficulty = 1f;
        public QuestRewardSettings reward;

        public virtual bool CanCreate(QuestContext context) => context.Difficulty >= minDifficulty;
        public abstract Quest Create(QuestContext context);
    }
}