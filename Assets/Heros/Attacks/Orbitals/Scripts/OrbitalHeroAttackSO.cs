using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heros.Attacks.Orbital
{
    [CreateAssetMenu(menuName = "Hero Attack/Orbital")]
    public class OrbitalHeroAttackSO : HeroAttackSO<OrbitalWeaponStatsAtLevel>
    {
        public override Type HeroAttackType => typeof(OrbitalHeroAttack);
        public OrbitalHeroAttackSpawn OrbitalHeroAttackSpawnPrefab;

        public Transform targetTransformPrefab;

        public float distanceAwayFromHero;
        public float rotationSpeed;
        public bool destroyProjectiles;

        public int amountOfHits;
        public bool projectilesCountToHits;
        public bool clockwise;


        public override Buff GetBuffAtIndex(HeroStats heroStats, int index)
        {
            return heroStats switch
            {
                HeroStats.NumberOfAttacks => WeaponStatsAtLevel[index].numberOfOrbitals,
                HeroStats.ProjectileTurnSpeed => WeaponStatsAtLevel[index].orbitalSpeed,
                _ => null,
            };
        }
    }
}

