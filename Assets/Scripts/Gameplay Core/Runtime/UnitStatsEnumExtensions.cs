using System;

namespace Units
{
    public static class UnitStatsEnumExtensions
    {
        public static bool IsIntStat(this UnitStatsEnum stat)
        {
            return stat switch
            {
                UnitStatsEnum.Health => true,
                UnitStatsEnum.Shield => true,

                _ => false,
            };
        }

        public static StatTypes ConvertToAllStats(this UnitStatsEnum unitStat)
        {
            if (Enum.TryParse(unitStat.ToString(), out StatTypes returnValue))
            {
                return returnValue;
            }

            throw new Exception("Could not parse UnitStat to AllStat");
        }
    }
}
