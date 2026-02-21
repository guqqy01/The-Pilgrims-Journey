using System;

namespace Tiles
{
    public static class TileEventBus
    {
        public static event Action<Tile> OnTileSelected;
        public static event Action<Tile> OnDragTile;

        public static void RaiseTileSelected(Tile tile)
            => OnTileSelected?.Invoke(tile);

        public static void RaiseDragTile(Tile tile)
            => OnDragTile?.Invoke(tile);

        public static void Clear()
        {
            OnTileSelected = null;
            OnDragTile = null;
        }
    }
}
