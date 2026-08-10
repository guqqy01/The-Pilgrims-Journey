using Units;

public class OnHitInfo
{
    public readonly HealthAndShieldManager HealthAndShieldManager;
    public readonly bool HitWasKilled;
    public readonly int DamageToShield;
    public readonly int DamageToHealth;

    public int TotalDamage => DamageToShield + DamageToHealth;

    public OnHitInfo(HealthAndShieldManager healthAndShieldManager, bool hitWasKilled, int dmgToShield, int dmgToHealth)
    {
        HealthAndShieldManager = healthAndShieldManager;
        HitWasKilled = hitWasKilled;
        DamageToShield = dmgToShield;
        DamageToHealth = dmgToHealth;
    }
}

