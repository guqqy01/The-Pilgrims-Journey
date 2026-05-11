using System;

namespace Tiles
{
    public static class TileUIEventBus<T>
    {
        public static event Action<TileUI<T>> OnTileSelected;
        public static event Action<TileUI<T>> OnDragTile;

        public static void RaiseTileSelected(TileUI<T> tile)
            => OnTileSelected?.Invoke(tile);

        public static void RaiseDragTile(TileUI<T> tile)
            => OnDragTile?.Invoke(tile);

        public static void Clear()
        {
            OnTileSelected = null;
            OnDragTile = null;
        }
    }
}
