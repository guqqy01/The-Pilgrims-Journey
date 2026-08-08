namespace Attacks
{

    public class AttackStats
    {
        public int Damage;
        public float CriticalChance;
        public float CriticalDamageMultiplier;
        public float Knockback;
        public float HitRefreshSpeed;
        public bool DestroyProjectiles;
        public int? AmountOfHits;
        public bool ProjectilesCountAsHits;

        public AttackStats(int damage, float criticalChance, float criticalDamageMultiplier, float knockback, float hitRefreshSpeed, bool destroyProjectiles = false, int? amountOfHits = null, bool projectilesCountAsHits = false)
        {
            Damage = damage;
            CriticalChance = criticalChance;
            CriticalDamageMultiplier = criticalDamageMultiplier;
            Knockback = knockback;
            HitRefreshSpeed = hitRefreshSpeed;
            DestroyProjectiles = destroyProjectiles;
            AmountOfHits = amountOfHits;
            ProjectilesCountAsHits = projectilesCountAsHits;
        }

        public AttackStats(AttackStats other)
        {
            Damage = other.Damage;
            CriticalChance = other.CriticalChance;
            CriticalDamageMultiplier = other.CriticalDamageMultiplier;
            Knockback = other.Knockback;
            HitRefreshSpeed = other.HitRefreshSpeed;
            DestroyProjectiles = other.DestroyProjectiles;
            AmountOfHits = other.AmountOfHits;
            ProjectilesCountAsHits = other.ProjectilesCountAsHits;
        }
    }

}
