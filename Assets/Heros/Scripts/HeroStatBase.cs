namespace Heros
{
    public static class HeroStatBase
    {
        internal static BuffableStat GetBuffableStat(HeroSO heroSO, HeroStats heroStat)
        {
            if(heroSO == null) return null;

            BuffableStat returnStat;
            if (heroStat.IsIntStat())
                returnStat = new BuffableStat(0);
            else
                returnStat = new BuffableStat(0f);

            returnStat.AddBuff("HeroBase", heroSO.GetBuff(heroStat));
            if(HeroStatBuffsManager.Instance != null)
                returnStat.AddBuff("HeroStatsManager", HeroStatBuffsManager.Instance.GetBuff(heroStat));
            if(HeroEquipmentManager.Instance != null) 
                returnStat.AddBuff("HeroEquipmentManager", HeroEquipmentManager.Instance.GetBuff(heroStat));

            return returnStat;
        }
    }
}

