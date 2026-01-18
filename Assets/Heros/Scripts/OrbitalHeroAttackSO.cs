using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heros
{
    public class OrbitalHeroAttackSO : HeroAttackSO
    {
        public override Type HeroAttackType => typeof(OrbitalHeroAttack);
        [SerializeField]
        private List<OrbitalWeaponStatsAtLevel> weaponStats = new List<OrbitalWeaponStatsAtLevel>();
        public override List<WeaponStatsAtLevel> WeaponStats => weaponStats.Select(x => x as WeaponStatsAtLevel).ToList();
        public OrbitalHeroAttackSpawn OrbitalHeroAttackSpawnPrefab;

        public Transform targetTransformPrefab;

        public float distanceAwayFromHero;
        public float rotationSpeed;
        public bool destroyProjectiles;

        public int amountOfHits;
        public bool projectilesCountToHits;

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
                HeroStats.NumberOfAttacks => weaponStats[index].numberOfOrbitals,
                HeroStats.ProjectileTurnSpeed => weaponStats[index].orbitalSpeed,
                _ => null,
            };
        }
    }
}

