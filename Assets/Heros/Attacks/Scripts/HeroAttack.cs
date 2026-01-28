namespace Heros.Attacks
{

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
