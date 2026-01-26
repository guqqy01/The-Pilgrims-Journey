using System.Collections.Generic;
using UnityEngine;

namespace Heros.Attacks.Spot
{
    public abstract class SpotHeroAttackSOBase : HeroAttackSO
    {
        public SpotChaoticMovementStats spotChaoticMovementStats;
        public float attackRange;

        public bool destroyProjectiles;
        public int amountOfHits;
        public bool projectilesCountToHits;
        public float lifeTime;

        public List<SpotWeaponStatsAtLevel> weaponStats = new List<SpotWeaponStatsAtLevel>();

        public abstract GameObject SpawnPrefab { get; }
    }
}
