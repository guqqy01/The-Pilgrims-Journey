namespace Heros.Attacks.Spot
{
    public class HeroMovementSpotAttackStatsContext : SpotAttackStatsContext
    {
        public readonly HeroMovementSpotAttackStats heroMovementSpotAttackStats;
        public HeroMovementSpotAttackStatsContext(  HeroAttackStats heroAttackStats, 
                                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                                        HeroMovementSpotAttackStats heroMovementSpotAttackStats) : 
                                                        base(heroAttackStats, spotChaoticMovementStats)
        {
            this.heroMovementSpotAttackStats = heroMovementSpotAttackStats;
        }
    }
}