using System;

namespace Tiles
{
    public static class TileUIEventBus
    {
        public static event Action<TileUIBase> OnTileSelected;
        public static event Action<TileUIBase> OnDragTile;

        public static void RaiseTileSelected(TileUIBase tile)
            => OnTileSelected?.Invoke(tile);

        public static void RaiseDragTile(TileUIBase tile)
            => OnDragTile?.Invoke(tile);

        public static void Clear()
        {
            OnTileSelected = null;
            OnDragTile = null;
        }
    }
}
