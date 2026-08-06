namespace Heros.Attacks.Spot
{
    public class ToMonsterMovementSpotAttackStatsContext : RandomMovementSpotAttackStatsContext
    {
        public readonly ToMonsterMovementStats toMonsterMovementStats;

        public ToMonsterMovementSpotAttackStatsContext(AttackStats heroAttackStats,
                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                        RandomMovementStats randomMovementStats,
                                        ToMonsterMovementStats toMonsterMovementStats) :
                                        base(heroAttackStats, spotChaoticMovementStats, randomMovementStats)
        {
            this.toMonsterMovementStats = toMonsterMovementStats;
        }
    }
}