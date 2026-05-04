using System;

namespace Heros.Attacks
{
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