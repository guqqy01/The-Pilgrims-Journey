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

        public SpotAttackSpawn spotAttackSpawnPrefab;

        public SpotMovementType movementType;

        public float chaoticMovementRange;

        public override Buff GetBuff(HeroStats heroStats, int level)
        {
            int index = level - 1;

            if (index < 0 || index >= weaponStats.Count)
                return null;

            return heroStats switch
            {
                HeroStats.Damage => weaponStats[index].damage,
                HeroStats.AttackSpeed => weaponStats[index].attackSpeed,
                HeroStats.CriticalChance => weaponStats[index].criticalChance,
                HeroStats.CriticalDamageMultiplier => weaponStats[index].criticalDamageMultiplier,
                HeroStats.Knockback => weaponStats[index].knockback,
                HeroStats.AttackSize => weaponStats[index].size,
                HeroStats.Accuracy => weaponStats[index].spawnAccuracy,
                _ => null,
            };
        }
    }

    public enum SpotMovementType
    {
        Still,
        Random,
        ToHero,
        FromHero,
        ToMonster,
    }

    public class SpotAttackSpawn : MonoBehaviour
    {
        [SerializeField]
        private new Collider2D collider2D;

        [SerializeField]
        private LayerMask hitLayerMask;

        private readonly Dictionary<HealthAndShieldManager, float> timeToNextDmg = new Dictionary<HealthAndShieldManager, float>();

        private readonly List<HealthAndShieldManager> toRemove = new List<HealthAndShieldManager>();

        private int damage;
        private float knockback;
        private float orbitSpeed;
        private float attackSpeed;
        private bool destroyProjectiles;
        private int currentAmountOfHits;
        private int? amountOfHits;
        private bool projectilesCountAsHits;
    }

    public class SpotHeroAttack : HeroAttack
    {
        public BuffableStat spawnAccuracy;

        protected override bool Attack()
        {
            throw new NotImplementedException();
        }

        protected override void InheritLevelSetup()
        {
            throw new NotImplementedException();
        }

        protected override void InheritSetup(HeroSO heroSO, HeroAttackSO heroAttackSO)
        {
            throw new NotImplementedException();
        }
    }
}
