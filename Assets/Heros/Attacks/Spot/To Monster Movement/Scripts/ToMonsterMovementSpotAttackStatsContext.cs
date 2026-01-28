namespace Heros.Attacks.Spot
{
    public class ToMonsterMovementSpotAttackStatsContext : RandomMovementSpotAttackStatsContext
    {
        public readonly ToMonsterMovementStats toMonsterMovementStats;

        public ToMonsterMovementSpotAttackStatsContext(HeroAttackStats heroAttackStats,
                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                        RandomMovementStats randomMovementStats,
                                        ToMonsterMovementStats toMonsterMovementStats) :
                                        base(heroAttackStats, spotChaoticMovementStats, randomMovementStats)
        {
            this.toMonsterMovementStats = toMonsterMovementStats;
        }
    }
}