namespace Attacks.Orbitals
{
    public class OrbitalAttackStatsContext : AttackStats
    {
        public readonly float orbitSpeed;

        public OrbitalAttackStatsContext(int damage, float criticalChance, float criticalDamageMultiplier, float orbitSpeed, float knockback, float hitSpeed, bool destroyProjectiles = false, int? amountOfHits = null, bool projectilesCountAsHit = false) : 
            base(damage, criticalChance, criticalDamageMultiplier, knockback, hitSpeed, destroyProjectiles, amountOfHits, projectilesCountAsHit)
        {
            this.orbitSpeed = orbitSpeed;
        }
    }
}

