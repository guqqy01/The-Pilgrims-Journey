using System.Collections.Generic;
using Tiles;

namespace TPJ.Gameplay
{
    /// <summary>
    /// Tracks which DeckHolder placed a given tile. TileBase itself stays unaware of the
    /// card game, so ownership lives here instead. Wire TileGridBase.onTileDestroyed (or
    /// each tile's ActionDestroyTile) to Remove() so this doesn't leak placed-then-destroyed tiles.
    /// </summary>
    public static class TileOwnership
    {
        private static readonly Dictionary<TileBase, DeckHolder> owners = new();

        public static void Set(TileBase tile, DeckHolder owner) => owners[tile] = owner;
        public static DeckHolder Get(TileBase tile) => owners.TryGetValue(tile, out var owner) ? owner : null;
        public static void Remove(TileBase tile) => owners.Remove(tile);
        public static void Clear() => owners.Clear();
    }
}