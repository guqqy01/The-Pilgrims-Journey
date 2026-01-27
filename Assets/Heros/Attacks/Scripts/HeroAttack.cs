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

    public abstract class HeroAttack<THeroAttackSO> : BaseHeroAttack where THeroAttackSO : BaseHeroAttackSO
    {
        public THeroAttackSO HeroAttackSO { get; private set; }

        public override BaseHeroAttackSO BaseHeroAttackSO => HeroAttackSO;

        protected abstract void InheritSetup(HeroSO heroSO, THeroAttackSO heroAttackSO);

        public override void Setup<TBaseHeroAttackSO>(HeroSO heroSO, TBaseHeroAttackSO heroAttackSO)
        {
            HeroAttackSO = heroAttackSO as THeroAttackSO;

            Level = 1;

            SetupBuffableStat(ref damage, HeroStats.Damage);
            SetupBuffableStat(ref attackSpeed, HeroStats.AttackSpeed);
            SetupBuffableStat(ref criticalChance, HeroStats.CriticalChance);
            SetupBuffableStat(ref criticalDamageMultiplier, HeroStats.CriticalDamageMultiplier);
            SetupBuffableStat(ref knockback, HeroStats.Knockback);
            SetupBuffableStat(ref size, HeroStats.AttackSize);

            findTargetDistance = heroAttackSO.findTargetDistance;

            enemyLayerMask = heroAttackSO.enemyLayerMask;

            InheritSetup(heroSO, HeroAttackSO);
        }

        protected void SetupBuffableStat(ref BuffableStat stat, HeroStats heroStats, AddBuffParameters addBuffParameters = AddBuffParameters.All)
        {
            stat = new BuffableStat(heroStats.IsIntStat() ? 0 : 0f);
            stat.AddBuff("Weapon", HeroAttackSO.GetBuff(heroStats, Level), addBuffParameters);
            stat.AddBuff("Hero", HeroManager.Instance.Stats.Get(heroStats), addBuffParameters);
        }
    }
}
