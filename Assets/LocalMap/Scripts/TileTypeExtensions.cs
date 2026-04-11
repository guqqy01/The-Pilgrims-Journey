namespace LocalMap
{
    public static class TileTypeExtensions
    {
        public static bool IsWalkable(this TileType tileType)
        {
            return tileType switch
            {
                TileType.Water => false,
                TileType.Nothing => false,
                _ => true
            };
        }
    }
}


