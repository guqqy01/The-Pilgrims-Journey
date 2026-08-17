using System;

namespace Tiles
{
    public static class TileGridTransferEvents
    {
        // tile, fromGrid, toGrid (toGrid == null means "no destination found, needs cleanup")
        public static event Action<TileBase, TileGridBase, TileGridBase> TileTransferred;

        public static void Raise(TileBase tile, TileGridBase from, TileGridBase to)
            => TileTransferred?.Invoke(tile, from, to);
    }
}

