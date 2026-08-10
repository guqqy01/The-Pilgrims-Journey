using Heros;
using UnityEngine;
namespace Attacks
{
    public abstract class Attack<TAttackSO> : BaseAttack where TAttackSO : BaseAttackSO
    {
        public TAttackSO AttackSO { get; private set; }

        public override BaseAttackSO BaseAttackSO => AttackSO;

        protected abstract void InheritSetup(TAttackSO heroAttackSO);

        public override void Setup<TBaseAttackSO>(TBaseAttackSO attackSO, LayerMask hitLayerMask)
        {
            AttackSO = attackSO as TAttackSO;

            Level = 1;

            SetupBuffableStat(ref amountOfHits, StatTypes.AttackDurability);
            SetupBuffableStat(ref damage, StatTypes.Damage);
            SetupBuffableStat(ref attackSpeed, StatTypes.AttackSpeed);
            SetupBuffableStat(ref criticalChance, StatTypes.CriticalChance);
            SetupBuffableStat(ref criticalDamageMultiplier, StatTypes.CriticalDamageMultiplier);
            SetupBuffableStat(ref knockback, StatTypes.Knockback);
            SetupBuffableStat(ref size, StatTypes.AttackSize);

            findTargetDistance = attackSO.findTargetDistance;

            this.hitLayerMask = hitLayerMask;

            InheritSetup(AttackSO);
        }

        protected void SetupBuffableStat(ref BuffableStat stat, StatTypes heroStats, AddBuffParameters addBuffParameters = AddBuffParameters.All)
        {
            stat = new BuffableStat(heroStats.IsIntStat() ? 0 : 0f);
            stat.AddBuff("Weapon", AttackSO.GetBuff(heroStats, Level), addBuffParameters);
            stat.AddBuff("Hero", HeroManager.Instance.Stats.Get(heroStats), addBuffParameters);
        }
    }
}
