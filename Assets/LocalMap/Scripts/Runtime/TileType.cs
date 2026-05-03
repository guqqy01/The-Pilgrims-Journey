using System;

namespace LocalMap
{
    [Flags]
    public enum TileType
    {
        Nothing = 0,
        Water = 1,
        StonePath = 2,
        Mud = 4,
        Dirt = 8,
        Sand = 16,
        Grass = 32,
    }
}
