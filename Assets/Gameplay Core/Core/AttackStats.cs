public struct AttackStats
{
    public int damage;

    public float criticalChance;
    public int criticalDamageMultiplier;

    public float knockbackMultiplier;

    public AttackStats(int damage, float criticalChance, int criticalDamageMultiplier, float knockbackMultiplier)
    {
        this.damage = damage;
        this.criticalChance = criticalChance;
        this.criticalDamageMultiplier = criticalDamageMultiplier;
        this.knockbackMultiplier = knockbackMultiplier;
    }
}


