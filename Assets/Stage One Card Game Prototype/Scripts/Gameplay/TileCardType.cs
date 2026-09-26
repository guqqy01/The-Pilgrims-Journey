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
        //Generates Action Points (Generation = Size of tile * 1)
        Farmer = 1,
        //Defends Farmers from Bandits and Scouts from spys (Defends only adjacent tiles)
        Guard = 2,
        //Steals farmer generation (Stops Farmers own generation and takes its action points. blocked if a farmer is defended by a guard)
        Bandit = 4,
        //Draws cards
        Scout = 8,
        //Steals scount tile card generation
        Spy = 16,
        //Destroy Tiles (Destroys one tile Adjacent to placement. Player choice after end turn so tile is locked.)
        Brigade = 32,
    }
}