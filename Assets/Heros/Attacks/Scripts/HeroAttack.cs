using UnityEngine;

namespace Heros
{
    public abstract class HeroAttack : MonoBehaviour
    {
        public HeroAttackSO HeroAttackSO { get; private set; }

        public int Level { private set; get; } = 1;

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

        protected abstract void InheritLevelSetup();

        public void SetHeroAttackLevel(int level)
        {
            Level = Mathf.Clamp(level, 1, HeroAttackSO.MaxLevel);

            damage.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.Damage, Level));
            attackSpeed.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.AttackSpeed, Level));
            criticalChance.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.CriticalChance, Level));
            criticalDamageMultiplier.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.CriticalDamageMultiplier, Level));
            knockback.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.Knockback, Level));
            size.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.AttackSize, Level));

            InheritLevelSetup();
        }

        public void Setup(HeroSO heroSO, HeroAttackSO heroAttackSO)
        {
            HeroAttackSO = heroAttackSO;

            Level = 1;

            SetupBuffableStat(ref damage, HeroStats.Damage);
            SetupBuffableStat(ref attackSpeed, HeroStats.AttackSpeed);
            SetupBuffableStat(ref criticalChance, HeroStats.CriticalChance);
            SetupBuffableStat(ref criticalDamageMultiplier, HeroStats.CriticalDamageMultiplier);
            SetupBuffableStat(ref knockback, HeroStats.Knockback);
            SetupBuffableStat(ref size, HeroStats.AttackSize);

            findTargetDistance = heroAttackSO.findTargetDistance;

            enemyLayerMask = heroAttackSO.enemyLayerMask;

            InheritSetup(heroSO, heroAttackSO);
        }

        protected void SetupBuffableStat(ref BuffableStat stat, HeroStats heroStats)
        {
            stat = new BuffableStat(heroStats.IsIntStat() ? 0 : 0f);
            stat.AddBuff("Weapon", HeroAttackSO.GetBuff(heroStats, Level));
            stat.AddBuff("Hero", HeroManager.Instance.Stats.Get(heroStats));
        }
    }

    public class FireCircleHeroAttack : HeroAttack
    {
        protected override bool Attack()
        {
            
        }

        protected override void InheritLevelSetup()
        {
            
        }

        protected override void InheritSetup(HeroSO heroSO, HeroAttackSO heroAttackSO)
        {
            
        }
    }
}
