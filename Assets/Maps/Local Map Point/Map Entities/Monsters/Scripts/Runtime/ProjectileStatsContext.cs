using Heros.Attacks;

// ProjectileStatsContext adapts gameplay ProjectileStats / AttackStats into the AttackStatsContext used by AttackSpawn
public class ProjectileStatsContext : AttackStatsContext
{
    public readonly float speed;
    public readonly float turningSpeed;

    public ProjectileStatsContext(ProjectileStats projectileStats)
        : base(projectileStats.Damage, 
            projectileStats.CriticalChance, 
            projectileStats.CriticalDamageMultiplier, 
            projectileStats.Knockback,
            projectileStats.HitRefreshSpeed, 
            projectileStats.DestroyProjectiles, 
            projectileStats.AmountOfHits, 
            projectileStats.ProjectilesCountAsHits)
    {
        speed = projectileStats.Speed;
        turningSpeed = projectileStats.TurningSpeed;
    }

    public ProjectileStatsContext(AttackStats gameplayAttackStats, float speed, float turningSpeed)
        : base(gameplayAttackStats.Damage,
            gameplayAttackStats.CriticalChance,
            gameplayAttackStats.CriticalDamageMultiplier,
            gameplayAttackStats.Knockback,
            gameplayAttackStats.HitRefreshSpeed,
            gameplayAttackStats.DestroyProjectiles,
            gameplayAttackStats.AmountOfHits,
            gameplayAttackStats.ProjectilesCountAsHits)
    {
        this.speed = speed;
        this.turningSpeed = turningSpeed;
    }
}
