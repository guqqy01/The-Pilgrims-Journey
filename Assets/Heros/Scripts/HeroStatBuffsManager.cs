using UnityEngine;
using System;
using Helper.Wait;
using Equipment;
using System.Collections.Generic;

namespace Heros
{
    public class HeroStatBuffsManager : Singleton<HeroStatBuffsManager>
    {
        private HeroStatContainer container;

        public delegate void StatsChanged();
        public StatsChanged statsChanged;

        public void Bind(HeroStatContainer container)
        {
            this.container = container;
        }

        public void AddBuff(string reference, HeroStats stat, Buff buff)
        {
            container.Get(stat).AddBuff(reference, buff);

            statsChanged?.Invoke();
        }

        public void RemoveBuff(string reference, HeroStats stat)
        {
            container.Get(stat).RemoveBuff(reference);

            statsChanged?.Invoke();
        }
    }
}


