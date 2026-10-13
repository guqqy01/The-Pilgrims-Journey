using Spawning;

namespace Quests
{
    public class QuestContext
    {
        public readonly float Difficulty;
        public readonly LevelPlan Plan;
        public readonly System.Random Random;
        public SpawnDirector Director => SpawnDirector.Instance;

        public QuestContext(float difficulty, LevelPlan plan, System.Random random)
        {
            Difficulty = difficulty;
            Plan = plan;
            Random = random;
        }
    }
}