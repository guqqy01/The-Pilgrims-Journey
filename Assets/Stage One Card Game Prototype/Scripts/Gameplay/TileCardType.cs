using System;

namespace TPJ.Gameplay
{
    public enum TileCardActivationType
    {
        OnPlay,
        OnTurn,
        CustomTrigger,
    }

    [Flags]
    public enum TileCardType
    {
        Farmer = 1,
        Guard = 2,
        Bandit = 4,
        Scout = 8,
        Traveler = 16,
        Brigade = 32,
    }
}