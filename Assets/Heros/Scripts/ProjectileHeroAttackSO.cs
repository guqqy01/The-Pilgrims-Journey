using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Heros
{
    [CreateAssetMenu(menuName = "Hero Attack/Projectile")]
    public class ProjectileHeroAttackSO : HeroAttackSO
    {
        public override Type HeroAttackType => typeof(ProjectileHeroAttack);

        [SerializeField]
        private List<ProjectileWeaponStatsAtLevel> projectileWeaponStatsAtLevels = new List<ProjectileWeaponStatsAtLevel>();
        public override List<WeaponStatsAtLevel> WeaponStats => projectileWeaponStatsAtLevels.Select(x => x as WeaponStatsAtLevel).ToList();

        public Projectile projectilePrefab;

        public bool projectileHoming;

        public override Buff GetBuff(HeroStats heroStats, int level)
        {
            return heroStats switch
            {
                HeroStats.Damage => projectileWeaponStatsAtLevels[level - 1].damage,
                HeroStats.AttackSpeed => projectileWeaponStatsAtLevels[level - 1].attackSpeed,
                HeroStats.CriticalChance => projectileWeaponStatsAtLevels[level - 1].criticalChance,
                HeroStats.CriticalDamageMultiplier => projectileWeaponStatsAtLevels[level - 1].criticalDamageMultiplier,
                HeroStats.Knockback => projectileWeaponStatsAtLevels[level - 1].knockback,
                HeroStats.NumberOfProjectiles => projectileWeaponStatsAtLevels[level - 1].numberOfProjectiles,
                HeroStats.AttackSize => projectileWeaponStatsAtLevels[level - 1].size,
                HeroStats.ProjectileSpeed => projectileWeaponStatsAtLevels[level - 1].projectileSpeed,
                HeroStats.ProjectileTurnSpeed => projectileWeaponStatsAtLevels[level - 1].projectileTurnSpeed,
                _ => null,
            };
        }   
    }
}

