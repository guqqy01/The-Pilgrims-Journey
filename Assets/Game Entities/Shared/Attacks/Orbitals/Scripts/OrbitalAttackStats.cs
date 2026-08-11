namespace Attacks.Orbitals
{
    public class OrbitalAttackStats : AttackStats
    {
        public readonly float orbitSpeed;

        public OrbitalAttackStats(
            int damage, 
            float size, 
            float criticalChance, 
            float criticalDamageMultiplier, 
            float orbitSpeed, 
            float knockback, 
            float hitSpeed, 
            bool destroyProjectiles = false, 
            int? amountOfHits = null, 
            bool projectilesCountAsHit = false) : 
                base(
                    damage, 
                    size, 
                    criticalChance, 
                    criticalDamageMultiplier, 
                    knockback, 
                    hitSpeed, 
                    destroyProjectiles, 
                    amountOfHits, 
                    projectilesCountAsHit)
        {
            this.orbitSpeed = orbitSpeed;
        }
    }
}

