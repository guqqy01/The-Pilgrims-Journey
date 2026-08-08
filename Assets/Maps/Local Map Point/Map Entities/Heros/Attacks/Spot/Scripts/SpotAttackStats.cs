namespace Attacks.Spot
{
    public class SpotAttackStats : AttackStats
    {
        public readonly SpotChaoticMovementStats spotChaoticMovementStats;
        public SpotAttackStats(AttackStats heroAttackStats, 
                                        SpotChaoticMovementStats spotChaoticMovementStats) : 
                                        base(heroAttackStats)
        {
            this.spotChaoticMovementStats = spotChaoticMovementStats;
        }
    }
}