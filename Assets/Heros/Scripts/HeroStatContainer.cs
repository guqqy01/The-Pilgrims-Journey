using System;
using System.Collections.Generic;
namespace Heros
{
    public class HeroStatContainer
    {
        private readonly Dictionary<HeroStats, BuffableStat> stats;

        public BuffableStat Get(HeroStats stat) => stats[stat];

        public delegate void OnStatChanged();
        public OnStatChanged statChanged;

        public HeroStatContainer(HeroSO heroSO)
        {
            stats = new Dictionary<HeroStats, BuffableStat>();

            foreach(HeroStats stat in Enum.GetValues(typeof(HeroStats)))
            {
                Buff baseBuff = heroSO.GetBuff(stat);
                if (baseBuff == null) continue;

                var buffable = new BuffableStat(stat.IsIntStat() ? 0 : 0f);

                buffable.onValueChange += () => statChanged?.Invoke();

                buffable.AddBuff("Hero Base", baseBuff);
                stats.Add(stat, buffable);
            }
        }
    }
}

