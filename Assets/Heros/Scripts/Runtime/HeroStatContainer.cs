using GameManagerCalls;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heros
{
    [Serializable]
    public class HeroStatContainer
    {
        private readonly Dictionary<HeroStats, BuffableStat> stats = new Dictionary<HeroStats, BuffableStat>();

        public BuffableStat Get(HeroStats stat)
        {
            if(stats.TryGetValue(stat, out var buff))
            {
                return buff;
            }
            else
            {
                return null;
            }
        }

        public delegate void OnStatChanged();
        public OnStatChanged statChanged;

        public HeroStatContainer(HeroSO heroSO)
        {
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

        // Ensure any stats missing from serialized or initial setup are created from the HeroSO defaults
        public void EnsureDefaults(HeroSO heroSO)
        {
            foreach(HeroStats stat in Enum.GetValues(typeof(HeroStats)))
            {
                if (stats.ContainsKey(stat))
                    continue;

                Buff baseBuff = null;
                try { baseBuff = heroSO.GetBuff(stat); } catch { baseBuff = null; }

                var buffable = new BuffableStat(stat.IsIntStat() ? 0 : 0f);
                buffable.onValueChange += () => statChanged?.Invoke();

                if (baseBuff != null)
                {
                    buffable.AddBuff("Hero Base", baseBuff);
                }

                stats[stat] = buffable;
            }
        }
    }
}

