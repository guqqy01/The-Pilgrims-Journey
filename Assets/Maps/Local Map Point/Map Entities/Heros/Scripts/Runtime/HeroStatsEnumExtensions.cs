using System;
namespace Heros
{
    public static class HeroStatsEnumExtensions
    {
        public static bool IsIntStat(this HeroStats stat)
        {
            return stat switch
            {
                HeroStats.Health => true,
                HeroStats.Shield => true,
                HeroStats.Damage => true,
                HeroStats.NumberOfAttacks => true,
                HeroStats.AttackDurability => true,
                _ => false,
            };
        }

        public static AllStats ConvertToAllStats(this HeroStats heroStat)
        {
            if(Enum.TryParse(heroStat.ToString(), out AllStats returnValue))
            {
                return returnValue;
            }

            throw new Exception("Could not parse heroStat to AllStat");
        }
    }
}