namespace Heros
{
    public static class HeroStatBase
    {
        internal static BuffableStat GetBuffableStat(HeroSO heroSO, HeroStats heroStat)
        {
            BuffableStat returnStat;
            if (heroStat.IsIntStat())
                returnStat = new BuffableStat(0);
            else
                returnStat = new BuffableStat(0f);

            returnStat.AddBuff("HeroBase", heroSO.GetBuff(heroStat));
            returnStat.AddBuff("HeroStatsManager", HeroStatsManager.Instance.GetBuff(heroStat));
            returnStat.AddBuff("HeroEquipmentManager", HeroEquipmentManager.Instance.GetBuff(heroStat));

            return returnStat;
        }
    }
}

