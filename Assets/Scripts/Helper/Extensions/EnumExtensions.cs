using Heros;
using Units;
public static class EnumExtensions
{
    public static bool IsIntStat(this HeroStats stat)
    {
        return stat switch
        {
            HeroStats.Health => true,
            HeroStats.Shield => true,
            HeroStats.Damage => true,
            HeroStats.NumberOfAttacks => true,

            _ => false,
        };
    }

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
