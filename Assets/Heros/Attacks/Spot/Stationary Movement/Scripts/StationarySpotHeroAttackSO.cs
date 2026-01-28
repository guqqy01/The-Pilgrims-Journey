using UnityEngine;

namespace Heros.Attacks.Spot
{
    [CreateAssetMenu(menuName = "Hero Attack/Spot/Stationary")]
    public class StationarySpotHeroAttackSO : SpotHeroAttackSO<StationarySpotHeroAttack, StationarySpotAttackSpawn, SpotAttackInitContext, SpotAttackStatsContext>
    {

    }
}
