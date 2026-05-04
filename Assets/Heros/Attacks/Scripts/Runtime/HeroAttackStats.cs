namespace Heros.Attacks
{
    public struct HeroAttackStats
    {
        public int damage;
        public float knockback;
        public float hitRefreshSpeed;
        public bool destroyProjectiles;
        public int? amountOfHits;
        public bool projectilesCountAsHits;

        public HeroAttackStats(int damage, float knockback, float hitRefreshSpeed, bool destroyProjectiles = false, int? amountOfHits = null, bool projectilesCountAsHit = false)
        {
            this.damage = damage;
            this.knockback = knockback;
            this.hitRefreshSpeed = hitRefreshSpeed;
            this.destroyProjectiles = destroyProjectiles;
            this.amountOfHits = amountOfHits;
            this.projectilesCountAsHits = projectilesCountAsHit;
        }
    }
}