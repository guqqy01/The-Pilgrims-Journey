using System;
namespace Heros
{
    public static class HeroStatsEnumExtensions
    {
        public static bool IsIntStat(this StatTypes stat)
        {
            return stat switch
            {
                StatTypes.Health => true,
                StatTypes.Shield => true,
                StatTypes.Damage => true,
                StatTypes.NumberOfAttacks => true,
                StatTypes.AttackDurability => true,
                _ => false,
            };
        }

        public static StatTypes ConvertToAllStats(this StatTypes heroStat)
        {
            if(Enum.TryParse(heroStat.ToString(), out StatTypes returnValue))
            {
                return returnValue;
            }

            throw new Exception("Could not parse heroStat to AllStat");
        }
    }
}