using Tiles;

namespace TPJ.Gameplay
{
    public abstract class TileCardTypeBehavior
    {
        public abstract TileCardType HandledType { get; }

        /// <summary>owner is the DeckHolder who just played this tile. Passed explicitly rather
        /// than read via TileOwnership.Get(tile) because ownership is recorded slightly after
        /// this fires — see CardGameEventsBus.RaiseOnTileCardPlayed's call site.</summary>
        public virtual void OnPlaced(DeckHolder owner, TileBase tile) { }
        public virtual void OnTurnBegin(TileBase tile) { }
    }
}