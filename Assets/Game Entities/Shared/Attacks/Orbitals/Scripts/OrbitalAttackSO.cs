using System;
using Heros;
using UnityEngine;

namespace Attacks.Orbitals
{
    [CreateAssetMenu(menuName = "Attack/Orbital")]
    public class OrbitalAttackSO : AttackSO<OrbitalAttackSpawn, OrbitalWeaponStatsAtLevel>
    {
        public override Type AttackType => typeof(OrbitalAttack);

        public Transform targetTransformPrefab;

        public float distanceAwayFromSender;
        public float rotationSpeed;
        public bool destroyProjectiles;

        public int amountOfHits;
        public bool projectilesCountToHits;
        public bool clockwise;


        public override Buff GetBuffAtIndex(StatTypes heroStats, int index)
        {
            return heroStats switch
            {
                StatTypes.NumberOfAttacks => WeaponStatsAtLevel[index].numberOfOrbitals,
                StatTypes.ProjectileTurnSpeed => WeaponStatsAtLevel[index].orbitalSpeed,
                _ => null,
            };
        }
    }
}

