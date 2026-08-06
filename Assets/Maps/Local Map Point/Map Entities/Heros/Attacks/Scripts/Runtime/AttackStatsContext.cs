namespace Heros.Attacks
{
    public class AttackStatsContext
    {
        public readonly AttackStats attackStats;

        public AttackStatsContext(int damage, float criticalChance, float criticalDamageMultiplier, float knockback, float hitSpeed, bool destroyProjectiles = false, int? amountOfHits = null, bool projectilesCountAsHits = false)
        {
            attackStats = new AttackStats(damage, criticalChance, criticalDamageMultiplier, knockback, hitSpeed, destroyProjectiles, amountOfHits, projectilesCountAsHits);
        }

        public AttackStatsContext(AttackStats attackStats)
        {
            this.attackStats = attackStats;
        }
    }
}
