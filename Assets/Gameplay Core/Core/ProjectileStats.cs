public struct ProjectileStats
{
    public AttackStats attackStats;

    public float speed;
    public float turningSpeed;

    public ProjectileStats(AttackStats attackStats, float speed, float turningSpeed)
    {
        this.attackStats = attackStats;

        this.speed = speed;
        this.turningSpeed = turningSpeed;
    }

    public ProjectileStats(int damage, float criticalChance, int criticalDamageMultiplier, float knockbackMultiplier, float speed, float turningSpeed)
    {
        attackStats = new AttackStats(damage, criticalChance, criticalDamageMultiplier, knockbackMultiplier);

        this.speed = speed;
        this.turningSpeed = turningSpeed;
    }
}


