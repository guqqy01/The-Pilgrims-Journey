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

        protected float nextAttackTime;

        protected float Cooldown => 1f / attackSpeed;

        [SerializeField]
        protected float knockbackMultiplier;

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

        protected abstract void InheritSetup(HeroAttackSO heroAttackSO);

        public void Setup(HeroAttackSO heroAttackSO)
        {
            HeroAttackSO = heroAttackSO;

            damage = new BuffableStat(heroAttackSO.damage);
            damage.AddBuff("HeroStatsManager", HeroStatsManager.Instance.damageBuff);

            attackSpeed = new BuffableStat(heroAttackSO.attackSpeed);

            knockbackMultiplier = heroAttackSO.knockback;
            findTargetDistance = heroAttackSO.findTargetDistance;

            enemyLayerMask = heroAttackSO.enemyLayerMask;

            InheritSetup(heroAttackSO);
        }
    }
}
