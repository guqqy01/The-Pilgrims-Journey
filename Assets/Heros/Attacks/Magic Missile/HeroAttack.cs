using UnityEngine;

namespace Heros
{
    public abstract class HeroAttack : MonoBehaviour
    {
        public HeroAttackSO HeroAttackSO { get; private set; }

        [SerializeField]
        protected BuffableStat damage;
        [SerializeField]
        protected BuffableStat attackSpeed;

        [SerializeField]
        protected BuffableStat criticalChance;
        [SerializeField]
        protected BuffableStat criticalDamageMultiplier;

        [SerializeField]
        protected BuffableStat size;

        protected float nextAttackTime;

        protected float Cooldown => 1f / attackSpeed;

        [SerializeField]
        protected BuffableStat knockback;

        [SerializeField]
        protected LayerMask enemyLayerMask;

        [SerializeField]
        protected float findTargetDistance;

        protected virtual Transform GetTarget()
        {
            Collider2D[] collliders = Physics2D.OverlapCircleAll(transform.position, findTargetDistance, enemyLayerMask);

            float minDistance = float.MaxValue;
            Transform target = null;

            foreach (Collider2D collider in collliders)
            {
                float distance = Vector2.Distance(collider.transform.position, transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    target = collider.transform;
                }
            }

            return target;
        }

        protected virtual void Update()
        {
            if (Time.time >= nextAttackTime)
            {
                if (Attack())
                {
                    nextAttackTime = Time.time + Cooldown;
                }
            }
        }

        protected abstract bool Attack();

        protected abstract void InheritSetup(HeroSO heroSO, HeroAttackSO heroAttackSO);

        public void Setup(HeroSO heroSO, HeroAttackSO heroAttackSO)
        {
            HeroAttackSO = heroAttackSO;

            SetupBuffableStat(ref damage, HeroStats.Damage, heroAttackSO);
            SetupBuffableStat(ref attackSpeed, HeroStats.AttackSpeed, heroAttackSO);
            SetupBuffableStat(ref criticalChance, HeroStats.CriticalChance, heroAttackSO);
            SetupBuffableStat(ref criticalDamageMultiplier, HeroStats.CriticalDamageMultiplier, heroAttackSO);
            SetupBuffableStat(ref knockback, HeroStats.Knockback, heroAttackSO);
            SetupBuffableStat(ref size, HeroStats.AttackSize, heroAttackSO);

            findTargetDistance = heroAttackSO.findTargetDistance;

            enemyLayerMask = heroAttackSO.enemyLayerMask;

            InheritSetup(heroSO, heroAttackSO);
        }

        private void SetupBuffableStat(ref BuffableStat stat, HeroStats heroStats, HeroAttackSO heroAttackSO)
        {
            stat = new BuffableStat(heroStats.IsIntStat() ? 0 : 0f);
            stat.AddBuff("Weapon Base", heroAttackSO.GetBuff(heroStats));
            stat.AddBuff("Hero Base", HeroManager.Instance.Stats.Get(heroStats));
        }
    }
}
