using UnityEngine;
namespace Heros.Attacks.Spot
{
    [CreateAssetMenu(menuName = "Hero Attack/Spot/Random Movement")]
    public class RandomMovementSpotHeroAttackSO : 
        SpotHeroAttackSO<RandomMovementSpotHeroAttack,
            BaseRandomMovementSpotAttackSpawn<RandomMovementSpotAttackStatsContext>, 
            SpotAttackInitContext, 
            RandomMovementSpotAttackStatsContext>
    {
        public Vector2 randomChangeMovementTimeRange;
    }

    //public abstract class BaseRandomMovementSpotHeroAttackSO : 
}