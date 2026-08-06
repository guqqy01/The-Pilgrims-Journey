namespace Heros.Attacks.Spot
{
    public class HeroMovementSpotAttackStatsContext : SpotAttackStatsContext
    {
        public readonly HeroMovementSpotAttackStats heroMovementSpotAttackStats;
        public HeroMovementSpotAttackStatsContext(  AttackStats heroAttackStats, 
                                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                                        HeroMovementSpotAttackStats heroMovementSpotAttackStats) : 
                                                        base(heroAttackStats, spotChaoticMovementStats)
        {
            this.heroMovementSpotAttackStats = heroMovementSpotAttackStats;
        }
    }
}