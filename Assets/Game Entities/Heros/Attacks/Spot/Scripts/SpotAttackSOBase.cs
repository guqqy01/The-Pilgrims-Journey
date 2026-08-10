using System.Collections.Generic;
using UnityEngine;

namespace Attacks.Spot
{
    public abstract class SpotAttackSOBase<TInit, TStats>: AttackSO<SpotAttackSpawn<TInit, TStats>, SpotWeaponStatsAtLevel>
        where TInit : SpotAttackInit
        where TStats : SpotAttackStats
    {
        public SpotChaoticMovementStats spotChaoticMovementStats;
        public float attackRange;

        public bool destroyProjectiles;
        public int amountOfHits;
        public bool projectilesCountToHits;
        public float lifeTime;
        public float movementSpeed;
    }
}
