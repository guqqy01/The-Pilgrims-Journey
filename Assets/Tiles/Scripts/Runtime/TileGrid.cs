using System.Collections.Generic;
using UnityEngine;
using GameManagerCalls;

namespace Tiles
{

    public class TileGrid<TItem> : MonoBehaviour, IOnClear
    {
        public string SaveFileName => $"TileGrid_{name}_Save";

        [SerializeField]
        protected Vector2Int gridSize;
        public Vector2Int GridSize => gridSize;

        protected Tile<TItem>[,] tiles;
        public Tile<TItem>[,] Tiles
        {
            get
            {
                if(tiles == null)
                    tiles = new Tile<TItem>[gridSize.x, gridSize.y];

                return tiles;
            }
            set
            {
                tiles = value;
            }
        }

        public delegate void OnTileEvent(Tile<TItem> tile);
        public OnTileEvent onTileAdded;
        public OnTileEvent onTileRemoved;
        public OnTileEvent onTileDestroyed;

        private void OnDestroy()
        {
            onTileAdded = null;
            onTileRemoved = null;
            onTileDestroyed = null;
        }

        public Tile<TItem> this[int i, int j]
        {
            get
            {
                return Tiles[i, j];
            }
            set
            {
                Tiles[i, j] = value;
            }
        }

        public Tile<TItem> this[Vector2Int position]
        {
            get
            {
                return Tiles[position.x, position.y];
            }
            set
            {
                Tiles[position.x, position.y] = value;
            }
        }

        public List<Tile<TItem>> ListOfTiles
        {
            get
            {
                List<Tile<TItem>> returnList = new List<Tile<TItem>>();

                foreach (var tile in Tiles)
                {
                    if (tile != null && !returnList.Contains(tile))
                        returnList.Add(tile);
                }

                return returnList;
            }
        }

        public void Clear()
        {
            foreach (Tile<TItem> tile in ListOfTiles)
            {
                DestroyTile(tile);
            }
            Tiles = new Tile<TItem>[gridSize.x, gridSize.y];
        }

        public void DestroyTile(Tile<TItem> tile)
        {
            tile.ActionDestroyTile -= DestroyTile;
            tile.OnDestroy();
            onTileDestroyed?.Invoke(tile);
        }

        public bool PointOutOfGrid(Vector2Int point) => point.x >= gridSize.x || point.x < 0 || point.y >= gridSize.y || point.y < 0;

        public bool CanPlaceTile(Tile<TItem> tile, Vector2Int position, float rotation)
        {
            foreach (Vector2Int point in tile.Shape.Rotate(rotation))
            {
                if (PointOutOfGrid(point + position) ||
                    (TryGetTileAtPosition(point + position, out Tile<TItem> outTile) && outTile != tile))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryGetTileAtPosition(Vector2Int v2Int, out Tile<TItem> tile)
        {
            tile = null;

            if (PointOutOfGrid(v2Int))
                return false;

            tile = this[v2Int];

            return tile != null;
        }

        public bool TryPlaceTileAnywhere(Tile<TItem> tile)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (float rotation = 0; rotation < 360; rotation += 90)
                    {
                        if (TryPlaceTile(tile, new Vector2Int(x, y), rotation))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool TryPlaceTile(Tile<TItem> selectedTile, Vector2Int position, float rotation)
        {
            if (!CanPlaceTile(selectedTile, position, rotation))
                return false;

            RemoveTile(selectedTile);

            foreach (var point in selectedTile.Shape.Rotate(rotation).AddToAll(position))
            {
                Tiles[point.x, point.y] = selectedTile;
            }

            selectedTile.Rotation = rotation;
            selectedTile.PlacedPosition = position;

            selectedTile.TileGrid = this;

            selectedTile.ActionDestroyTile += DestroyTile;
            onTileAdded?.Invoke(selectedTile);
            return true;
        }

        public bool RemoveTile(Tile<TItem> tile)
        {
            bool haveRemovedTile = false;

            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    if (Tiles[i, j] == tile)
                    {
                        Tiles[i, j] = null;
                        haveRemovedTile = true;
                    }
                }
            }

            if (haveRemovedTile && tile.TileGrid != this)
            {
                tile.ActionDestroyTile -= DestroyTile;
                onTileRemoved?.Invoke(tile);
            }

            return haveRemovedTile;
        }

        

        
    }

}

