namespace Heros.Attacks.Spot
{
    public class RandomMovementSpotAttackStatsContext : SpotAttackStatsContext
    {
        public readonly RandomMovementStats randomMovementStats;

        public RandomMovementSpotAttackStatsContext(AttackStats heroAttackStats,
                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                        RandomMovementStats randomMovementStats) :
                                        base(heroAttackStats, spotChaoticMovementStats)
        {
            this.randomMovementStats = randomMovementStats;
        }
    }
}