using Monsters;

namespace Quests
{
    public class BossKilledQuest : Quest
    {
        private string bossName;
        private bool killed;

        private string BossName
        {
            get
            {
                if (bossName == null)
                {
                    Monster boss = MonsterSpawnManager.Instance != null ? MonsterSpawnManager.Instance.BossMonster : null;
                    if (boss != null)
                        bossName = boss.MonsterSO.name;
                }
                return bossName;
            }
        }

        public override string Title => BossName != null ? $"Defeat the {BossName}" : "Defeat the Boss";
        public override string Description => "Slay the boss guarding this relic.";
        public override string ProgressText => killed ? "Defeated" : "Alive";
        public override string FailureReason => "The boss was never defeated";
        protected override void OnBegin() => NotifyChanged(); // refresh the title now that the boss has likely spawned

        public override void OnBossKilled(Monster monster)
        {
            bossName ??= monster.MonsterSO.name;
            killed = true;
            Complete();
        }
    }
}