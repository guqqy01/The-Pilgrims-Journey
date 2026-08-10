namespace Attacks.Spot
{
    public class RandomMovementSpotAttackStats : SpotAttackStats
    {
        public readonly RandomMovementStats randomMovementStats;

        public RandomMovementSpotAttackStats(AttackStats heroAttackStats,
                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                        RandomMovementStats randomMovementStats) :
                                        base(heroAttackStats, spotChaoticMovementStats)
        {
            this.randomMovementStats = randomMovementStats;
        }
    }
}