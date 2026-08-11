namespace Attacks
{
    public class AttackStats
    {
        public int Damage;
        public float Size;
        public float CriticalChance;
        public float CriticalDamageMultiplier;
        public float Knockback;
        public float HitRefreshRate;
        public bool DestroyProjectiles;
        public int? AmountOfHits;
        public bool ProjectilesCountAsHits;

        public readonly bool HasExplicitStats = false;
        public AttackStats(int damage, float size, float criticalChance, float criticalDamageMultiplier, float knockback, float hitRefreshRate, bool destroyProjectiles = false, int? amountOfHits = null, bool projectilesCountAsHits = false)
        {
            HasExplicitStats = true;

            Damage = damage;
            Size = size;
            CriticalChance = criticalChance;
            CriticalDamageMultiplier = criticalDamageMultiplier;
            Knockback = knockback;
            HitRefreshRate = hitRefreshRate;
            DestroyProjectiles = destroyProjectiles;
            AmountOfHits = amountOfHits;
            ProjectilesCountAsHits = projectilesCountAsHits;
        }

        public AttackStats(AttackStats other)
        {
            HasExplicitStats = true;

            Damage = other.Damage;
            Size = other.Size;
            CriticalChance = other.CriticalChance;
            CriticalDamageMultiplier = other.CriticalDamageMultiplier;
            Knockback = other.Knockback;
            HitRefreshRate = other.HitRefreshRate;
            DestroyProjectiles = other.DestroyProjectiles;
            AmountOfHits = other.AmountOfHits;
            ProjectilesCountAsHits = other.ProjectilesCountAsHits;
        }
    }

}
