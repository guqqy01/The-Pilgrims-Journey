namespace Attacks.Projectiles
{
    public class ProjectileStats : AttackStats
    {
        public float Speed;
        public float TurningSpeed;

        public ProjectileStats(
            int damage,
            float size,
            float criticalChance,
            float criticalDamageMultiplier,
            float knockbackMultiplier,
            float speed,
            float turningSpeed,
            bool destroyProjectiles = false,
            int? amountOfHits = null,
            bool projectilesCountAsHits = false) :
                base(damage, size, criticalChance, criticalDamageMultiplier, knockbackMultiplier, 0, destroyProjectiles, amountOfHits, projectilesCountAsHits)
        {
            Speed = speed;
            TurningSpeed = turningSpeed;
        }
    }


}

