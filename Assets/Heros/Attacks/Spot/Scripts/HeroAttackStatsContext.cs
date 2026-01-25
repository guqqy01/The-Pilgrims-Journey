using System;

namespace Heros.Attacks
{
    [Serializable]
    public struct HeroAttackStats
    {
        public int damage;
        public float knockback;
        public float hitSpeed;
        public bool destroyProjectiles;
        public int? amountOfHits;
        public bool projectilesCountAsHits;

        public HeroAttackStats(int damage, float knockback, float hitSpeed, bool destroyProjectiles = false, int? amountOfHits = null, bool projectilesCountAsHit = false)
        {
            this.damage = damage;
            this.knockback = knockback;
            this.hitSpeed = hitSpeed;
            this.destroyProjectiles = destroyProjectiles;
            this.amountOfHits = amountOfHits;
            this.projectilesCountAsHits = projectilesCountAsHit;
        }
    }

    public class HeroAttackStatsContext
    {
        public readonly HeroAttackStats heroAttackStats;

        public HeroAttackStatsContext(int damage, float knockback, float hitSpeed, bool destroyProjectiles = false, int? amountOfHits = null, bool projectilesCountAsHit = false)
        {
            heroAttackStats = new HeroAttackStats(damage, knockback, hitSpeed, destroyProjectiles, amountOfHits, projectilesCountAsHit);
        }

        public HeroAttackStatsContext(HeroAttackStats heroAttackStats)
        {
            this.heroAttackStats = heroAttackStats;
        }
    }
}