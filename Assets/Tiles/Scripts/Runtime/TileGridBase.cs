using System.Collections.Generic;
using UnityEngine;
using GameManagerCalls;
namespace Tiles
{

    public abstract class TileGridBase : MonoBehaviour, IOnClear
    {
        public string SaveFileName => $"TileGrid_{name}_Save";

        [SerializeField]
        protected Vector2Int gridSize;
        public Vector2Int GridSize => gridSize;

        protected TileBase[,] tiles;
        public TileBase[,] Tiles
        {
            get
            {
                if(tiles == null)
                    tiles = new TileBase[gridSize.x, gridSize.y];

                return tiles;
            }
            set
            {
                tiles = value;
            }
        }

        public delegate void OnTileEvent(TileBase tile);
        public OnTileEvent onTileAdded;
        public OnTileEvent onTileRemoved;
        public OnTileEvent onTileDestroyed;

        public enum Axis
        {
            X,
            Y,
            MinusX,
            MinusY,
        }

        private void OnDestroy()
        {
            onTileAdded = null;
            onTileRemoved = null;
            onTileDestroyed = null;
        }

        public TileBase this[int i, int j]
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

        public TileBase this[Vector2Int position]
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

        public List<TileBase> ListOfTiles
        {
            get
            {
                List<TileBase> returnList = new List<TileBase>();

                foreach (var tile in Tiles)
                {
                    if (tile != null && !returnList.Contains(tile))
                        returnList.Add(tile);
                }

                return returnList;
            }
        }

        public abstract bool AcceptTile(TileBase tile);

        public void Organise(Axis axis)
        {
            List<TileBase> tileHolder = ListOfTiles;

            tiles = new TileBase[gridSize.x, gridSize.y];

            foreach(TileBase tile in tileHolder)
            {
                TryPlaceTileAnywhere(tile, axis, false);
            }
        }

        public void Clear()
        {
            foreach (TileBase tile in ListOfTiles)
            {
                DestroyTile(tile);
            }
            Tiles = new TileBase[gridSize.x, gridSize.y];
        }

        public void DestroyTile(TileBase tile)
        {
            tile.ActionDestroyTile -= DestroyTile;
            tile.OnDestroy();
            onTileDestroyed?.Invoke(tile);
        }

        public bool PointOutOfGrid(Vector2Int point) => point.x >= gridSize.x || point.x < 0 || point.y >= gridSize.y || point.y < 0;

        public bool TileOutOfGrid(TileBase tile, Vector2Int position, float rotation)
        {
            foreach (Vector2Int point in tile.Shape.Rotate(rotation))
            {
                if (PointOutOfGrid(point + position))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CanPlaceTile(TileBase tile, Vector2Int position, float rotation)
        {
            if (!AcceptTile(tile))
                return false;

            foreach (Vector2Int point in tile.Shape.Rotate(rotation))
            {
                if (PointOutOfGrid(point + position) ||
                    (TryGetTileAtPosition(point + position, out TileBase outTile) && outTile != tile))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryGetTileAtPosition(Vector2Int v2Int, out TileBase tile)
        {
            tile = null;

            if (PointOutOfGrid(v2Int))
                return false;

            tile = this[v2Int];

            return tile != null;
        }

        public bool TryPlaceTileAnywhere(TileBase tile, Axis axis, bool invokeAddedTile = true)
        {
            switch(axis)
            {
                case Axis.X:
                    return TryPlaceTileAnywhereX(tile, invokeAddedTile);
                case Axis.Y:
                    return TryPlaceTileAnywhereY(tile, invokeAddedTile);
                case Axis.MinusX:
                    return TryPlaceTileAnywhereMinusX(tile, invokeAddedTile);
                case Axis.MinusY:
                    return TryPlaceTileAnywhereMinusY(tile, invokeAddedTile);
                default:
                    return false;
            }
        }

        private bool TryPlaceTileAnywhereMinusX(TileBase tile, bool invokeAddedTile = true)
        {
            for (int y = gridSize.y - 1; y >= 0; y--)
            {
                for (int x = 0; x < gridSize.x ; x++)
                {
                    for (float rotation = 0; rotation < 360; rotation += 90)
                    {
                        if (TryPlaceTile(tile, new Vector2Int(x, y), rotation, invokeAddedTile))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool TryPlaceTileAnywhereMinusY(TileBase tile, bool invokeAddedTile = true)
        {
            for (int x = gridSize.x - 1; x >= 0; x--)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (float rotation = 0; rotation < 360; rotation += 90)
                    {
                        if (TryPlaceTile(tile, new Vector2Int(x, y), rotation, invokeAddedTile))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool TryPlaceTileAnywhereX(TileBase tile, bool invokeAddedTile = true)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (float rotation = 0; rotation < 360; rotation += 90)
                    {
                        if (TryPlaceTile(tile, new Vector2Int(x, y), rotation, invokeAddedTile))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool TryPlaceTileAnywhereY(TileBase tile, bool invokeAddedTile = true)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (float rotation = 0; rotation < 360; rotation += 90)
                    {
                        if (TryPlaceTile(tile, new Vector2Int(x, y), rotation, invokeAddedTile))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public virtual bool TryPlaceTile(TileBase selectedTile, Vector2Int position, float rotation, bool invokeAddedTile = true, bool invokeRemovedTile = true)
        {
            if (!CanPlaceTile(selectedTile, position, rotation))
                return false;

            RemoveTile(selectedTile, invokeRemovedTile);

            foreach (var point in selectedTile.Shape.Rotate(rotation).AddToAll(position))
            {
                Tiles[point.x, point.y] = selectedTile;
            }

            selectedTile.Rotation = rotation;
            selectedTile.PlacedPosition = position;

            selectedTile.TileGrid = this;

            selectedTile.ActionDestroyTile -= DestroyTile;
            selectedTile.ActionDestroyTile += DestroyTile;

            if(invokeAddedTile)
                onTileAdded?.Invoke(selectedTile);
            return true;
        }

        public bool RemoveTile(TileBase tile, bool invokeRemoveTile = true)
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

            if (haveRemovedTile)
            {
                tile.ActionDestroyTile -= DestroyTile;

                if(invokeRemoveTile)
                    onTileRemoved?.Invoke(tile);
            }

            return haveRemovedTile;
        }

        

        
    }

}

