namespace Attacks.Spot
{
    public class MovementSpotAttackStats : SpotAttackStats
    {
        public readonly float maxDistance;
        public MovementSpotAttackStats(  AttackStats heroAttackStats, 
                                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                                        float maxDistance) : 
                                                        base(heroAttackStats, spotChaoticMovementStats)
        {
            this.maxDistance = maxDistance;
        }
    }
}