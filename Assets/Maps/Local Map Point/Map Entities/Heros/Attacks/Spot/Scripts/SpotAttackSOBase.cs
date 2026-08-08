using System.Collections.Generic;
using UnityEngine;

namespace Attacks.Spot
{
    public abstract class SpotAttackSOBase : AttackSO<SpotWeaponStatsAtLevel>
    {
        public SpotChaoticMovementStats spotChaoticMovementStats;
        public float attackRange;

        public bool destroyProjectiles;
        public int amountOfHits;
        public bool projectilesCountToHits;
        public float lifeTime;
        public float movementSpeed;

        public abstract GameObject SpawnPrefab { get; }
    }
}
