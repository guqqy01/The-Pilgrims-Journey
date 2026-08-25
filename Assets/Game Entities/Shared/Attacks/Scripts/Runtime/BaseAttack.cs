using UnityEngine;
using Heros;
using System;
namespace Attacks
{
    public abstract class BaseAttack : MonoBehaviour
    {
        public abstract BaseAttackSO BaseAttackSO { get; }

        public bool Active = true;

        public int Level { protected set; get; } = 1;

        [SerializeField]
        protected BuffableStat damage;
        [SerializeField]
        protected BuffableStat hitRefreshRate;
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

        protected LayerMask hitLayerMask;

        [SerializeField]
        protected float findTargetDistance;

        protected bool destroyProjectiles;
        protected BuffableStat amountOfHits;
        protected bool projectilesCountAsHits;

        public bool AttacksTriggeredExternally;

        protected virtual Transform GetTarget()
        {
            Collider2D[] collliders = Physics2D.OverlapCircleAll(transform.position, findTargetDistance, hitLayerMask);

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
            if (!Active)
                return;

            if (Time.time >= nextAttackTime)
            {
                if (damage == null)
                    return;

                if (ProtectedAttack())
                {
                    ResetAttackTimer();
                }
            }
        }

        public void ExternalTriggerAttack()
        {
            if(!Active) 
                return;

            if (!AttacksTriggeredExternally)
                return;

            ProtectedAttack();
        }

        public void DelayAttack(float time)
        {
            nextAttackTime = Mathf.Max(Time.time, nextAttackTime) + time;
        }

        protected void ResetAttackTimer()
        {
            nextAttackTime = Time.time + Cooldown;
        }

        protected abstract bool ProtectedAttack();

        public abstract void Setup<TBaseAttackSO>(TBaseAttackSO attackSO, Func<StatTypes, Buff> entityBuffCallback, LayerMask hitLayerMask) 
            where TBaseAttackSO : BaseAttackSO;

        protected abstract void InheritLevelSetup();

        public void SetAttackLevel(int level)
        {
            Level = Mathf.Clamp(level, 1, BaseAttackSO.MaxLevel);

            if(amountOfHits != null)
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
