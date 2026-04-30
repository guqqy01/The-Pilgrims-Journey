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

                _ => false,
            };
        }
    }
}