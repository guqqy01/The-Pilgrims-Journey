namespace Heros.Attacks.Spot
{
    public class SpotAttackStatsContext : HeroAttackStatsContext
    {
        public readonly SpotChaoticMovementStats spotChaoticMovementStats;
        public SpotAttackStatsContext(  HeroAttackStats heroAttackStats, 
                                        SpotChaoticMovementStats spotChaoticMovementStats) : 
                                        base(heroAttackStats)
        {
            this.spotChaoticMovementStats = spotChaoticMovementStats;
        }
    }
}