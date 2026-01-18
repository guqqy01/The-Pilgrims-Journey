using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heros
{
    [CreateAssetMenu(menuName = "Hero Attack/Melee")]
    public class MeleeHeroAttackSO : HeroAttackSO
    {
        public override Type HeroAttackType => typeof(MeleeHeroAttack);
        [SerializeField]
        private List<WeaponStatsAtLevel> weaponStats = new List<WeaponStatsAtLevel>();
        public override List<WeaponStatsAtLevel> WeaponStats => weaponStats;

        public MeleeAttackSpawn meleeAttackSpawn;

        public bool destroyProjectiles;

        public override Buff GetBuff(HeroStats heroStats, int level)
        {
            int index = level - 1;
            return heroStats switch
            {
                HeroStats.Damage => weaponStats[index].damage,
                HeroStats.AttackSpeed => weaponStats[index].attackSpeed,
                HeroStats.CriticalChance => weaponStats[index].criticalChance,
                HeroStats.CriticalDamageMultiplier => weaponStats[index].criticalDamageMultiplier,
                HeroStats.Knockback => weaponStats[index].knockback,
                HeroStats.AttackSize => weaponStats[index].size,
                _ => null,
            };
        }
    }
}

