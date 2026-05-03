using System;

namespace Tiles
{
    public static class TileUIEventBus
    {
        public static event Action<TileUI> OnTileSelected;
        public static event Action<TileUI> OnDragTile;

        public static void RaiseTileSelected(TileUI tile)
            => OnTileSelected?.Invoke(tile);

        public static void RaiseDragTile(TileUI tile)
            => OnDragTile?.Invoke(tile);

        public static void Clear()
        {
            OnTileSelected = null;
            OnDragTile = null;
        }
    }
}
