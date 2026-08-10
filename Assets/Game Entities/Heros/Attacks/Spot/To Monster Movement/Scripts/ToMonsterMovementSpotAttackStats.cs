namespace Attacks.Spot
{
    public class ToMonsterMovementSpotAttackStats : RandomMovementSpotAttackStats
    {
        public readonly ToMonsterMovementStats toMonsterMovementStats;

        public ToMonsterMovementSpotAttackStats(AttackStats heroAttackStats,
                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                        RandomMovementStats randomMovementStats,
                                        ToMonsterMovementStats toMonsterMovementStats) :
                                        base(heroAttackStats, spotChaoticMovementStats, randomMovementStats)
        {
            this.toMonsterMovementStats = toMonsterMovementStats;
        }
    }
}