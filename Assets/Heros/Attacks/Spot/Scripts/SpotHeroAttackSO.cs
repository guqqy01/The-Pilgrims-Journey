using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heros.Attacks.Spot
{
    [CreateAssetMenu(menuName = "Hero Attack/Spot")]
    public class SpotHeroAttackSO : HeroAttackSO
    {
        public override Type HeroAttackType => typeof(SpotHeroAttack);
        [SerializeField]
        private List<SpotWeaponStatsAtLevel> weaponStats = new List<SpotWeaponStatsAtLevel>();
        public override List<WeaponStatsAtLevel> WeaponStats => weaponStats.Select(x => x as WeaponStatsAtLevel).ToList();
        public StationarySpotAttackSpawn spotAttackSpawnPrefab;

        public HeroSpotMovementType movementType;

        public float chaoticMovementRange;
        public float attackRange;

        public bool destroyProjectiles;
        public int amountOfHits;
        public bool projectilesCountToHits;

        public override Buff GetBuff(HeroStats heroStats, int level)
        {
            Buff baseBuff = base.GetBuff(heroStats, level);

            if (baseBuff != null)
                return baseBuff;

            int index = level - 1;

            if (index < 0 || index >= weaponStats.Count)
                return null;

            return heroStats switch
            {
                HeroStats.Accuracy => weaponStats[index].spawnAccuracy,
                _ => null,
            };
        }
    }
}
