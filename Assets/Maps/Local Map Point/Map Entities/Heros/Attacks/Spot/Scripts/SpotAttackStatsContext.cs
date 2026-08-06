namespace Heros.Attacks.Spot
{
    public class SpotAttackStatsContext : AttackStatsContext
    {
        public readonly SpotChaoticMovementStats spotChaoticMovementStats;
        public SpotAttackStatsContext(  AttackStats heroAttackStats, 
                                        SpotChaoticMovementStats spotChaoticMovementStats) : 
                                        base(heroAttackStats)
        {
            this.spotChaoticMovementStats = spotChaoticMovementStats;
        }
    }
}