using UnityEngine;
using Heros;
namespace Attacks
{
    public abstract class BaseAttack : MonoBehaviour
    {
        public abstract BaseAttackSO BaseAttackSO { get; }

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

        protected bool destroyProjectiles;
        [SerializeField]
        protected BuffableStat amountOfHits;
        protected bool projectilesCountAsHits;

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

        public abstract void Setup<TBaseAttackSO>(BuffableEntitySO buffableEntitySO, TBaseAttackSO attackSO) where TBaseAttackSO : BaseAttackSO;

        protected abstract void InheritLevelSetup();

        public void SetAttackLevel(int level)
        {
            Level = Mathf.Clamp(level, 1, BaseAttackSO.MaxLevel);

            amountOfHits.AddBuff("Weapon", BaseAttackSO.GetBuff(StatTypes.AttackDurability, Level));
            damage.AddBuff("Weapon", BaseAttackSO.GetBuff(StatTypes.Damage, Level));
            attackSpeed.AddBuff("Weapon", BaseAttackSO.GetBuff(StatTypes.AttackSpeed, Level));
            criticalChance.AddBuff("Weapon", BaseAttackSO.GetBuff(StatTypes.CriticalChance, Level));
            criticalDamageMultiplier.AddBuff("Weapon", BaseAttackSO.GetBuff(StatTypes.CriticalDamageMultiplier, Level));
            knockback.AddBuff("Weapon", BaseAttackSO.GetBuff(StatTypes.Knockback, Level));
            size.AddBuff("Weapon", BaseAttackSO.GetBuff(StatTypes.AttackSize, Level));

            InheritLevelSetup();
        }
    }
}
