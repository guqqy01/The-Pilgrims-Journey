using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heros.Attacks.Spot
{

    public class SpotHeroAttackSO<TAttack, TSpawn, TInit, TStats> : SpotHeroAttackSOBase
        where TAttack : SpotHeroAttack<TSpawn, TInit, TStats>
        where TSpawn : SpotAttackSpawn<TStats>
        where TInit : SpotAttackInitContext
        where TStats : SpotAttackStatsContext
    {
        public override Type HeroAttackType => typeof(TAttack);
        [SerializeField]
        private List<SpotWeaponStatsAtLevel> weaponStats = new List<SpotWeaponStatsAtLevel>();
        public override List<WeaponStatsAtLevel> WeaponStats => weaponStats.Select(x => x as WeaponStatsAtLevel).ToList();
        public TSpawn spotAttackSpawnPrefab;

        public override GameObject SpawnPrefab => spotAttackSpawnPrefab.gameObject;

        public override Buff GetBuff(HeroStats heroStats, int level)
        {
            Buff baseBuff = base.GetBuff(heroStats, level);

            if (baseBuff != null)
                return baseBuff;

            int index = level - 1;

            if (index < 0 || index >= weaponStats.Count)
                return null;

            return heroStats switch
            {
                HeroStats.Accuracy => weaponStats[index].spawnAccuracy,
                _ => null,
            };
        }
    }
}
