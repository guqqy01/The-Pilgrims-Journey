using UnityEngine;

namespace Heros.Attacks
{
    public abstract class BaseHeroAttack : MonoBehaviour
    {
        public abstract BaseHeroAttackSO BaseHeroAttackSO { get; }

        public int Level { protected set; get; } = 1;

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
                    ResetAttackTimer();
                }
            }
        }

        protected void ResetAttackTimer()
        {
            nextAttackTime = Time.time + Cooldown;
        }

        protected abstract bool Attack();

        public abstract void Setup<TBaseHeroAttackSO>(HeroSO heroSO, TBaseHeroAttackSO heroAttackSO) where TBaseHeroAttackSO : BaseHeroAttackSO;

        protected abstract void InheritLevelSetup();

        public void SetHeroAttackLevel(int level)
        {
            Level = Mathf.Clamp(level, 1, BaseHeroAttackSO.MaxLevel);

            damage.AddBuff("Weapon", BaseHeroAttackSO.GetBuff(HeroStats.Damage, Level));
            attackSpeed.AddBuff("Weapon", BaseHeroAttackSO.GetBuff(HeroStats.AttackSpeed, Level));
            criticalChance.AddBuff("Weapon", BaseHeroAttackSO.GetBuff(HeroStats.CriticalChance, Level));
            criticalDamageMultiplier.AddBuff("Weapon", BaseHeroAttackSO.GetBuff(HeroStats.CriticalDamageMultiplier, Level));
            knockback.AddBuff("Weapon", BaseHeroAttackSO.GetBuff(HeroStats.Knockback, Level));
            size.AddBuff("Weapon", BaseHeroAttackSO.GetBuff(HeroStats.AttackSize, Level));

            InheritLevelSetup();
        }
    }
}
