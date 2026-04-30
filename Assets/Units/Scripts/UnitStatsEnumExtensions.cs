
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
    }
}
