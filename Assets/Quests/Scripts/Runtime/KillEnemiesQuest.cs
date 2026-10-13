using Monsters;

namespace Quests
{
    // =====================================================================
    //  Kill enemies
    // =====================================================================

    public class KillEnemiesQuest : Quest
    {
        private readonly int target;
        private readonly MonsterSO only;
        private int kills;

        public KillEnemiesQuest(int target, MonsterSO only)
        {
            this.target = target;
            this.only = only;
        }

        public override string Title => only == null ? "Monster Hunter" : $"Hunt: {only.Title}";
        public override string Description => only == null ? $"Defeat {target} monsters." : $"Defeat {target} {only.Title}.";
        public override string ProgressText => $"{kills}/{target}";

        public override void OnMonsterKilled(Monster monster)
        {
            if (only != null && monster.MonsterSO != only)
                return;

            kills++;
            NotifyChanged();

            if (kills >= target)
                Complete();
        }
    }
}