using GameManagerCalls;
using System.Collections.Generic;
using TPJ.Gameplay;
using UnityEngine;
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
                if (tiles == null)
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
        // Fired when one cell of a tile is destroyed (e.g. by Brigade) but the tile itself
        // survives with a smaller footprint. Distinct from onTileRemoved, which only fires
        // when the tile is gone entirely.
        public OnTileEvent onTileSectionDestroyed;

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
            onTileSectionDestroyed = null;
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

            foreach (TileBase tile in tileHolder)
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

            RemoveTile(tile);
        }

        /// <summary>
        /// Destroys a single placed cell of `tile` (e.g. from a Brigade attack) rather than
        /// the whole tile. If this was the tile's last remaining active cell, it's a full
        /// destroy via the normal DestroyTile path instead — the tile object doesn't linger
        /// around empty. Returns false if `worldCell` doesn't actually belong to `tile`.
        /// </summary>
        public bool DestroyTileSection(TileBase tile, Vector2Int worldCell)
        {
            if (PointOutOfGrid(worldCell) || Tiles[worldCell.x, worldCell.y] != tile)
                return false;

            if (tile.ActivePlacedShape.Count <= 1)
            {
                DestroyTile(tile);
                return true;
            }

            List<TileBase> previouslyAdjacent = GetAdjacentTiles(tile);

            Tiles[worldCell.x, worldCell.y] = null;
            tile.MarkCellDestroyed(worldCell);

            CalculateAdjacentTileCardTypes(tile);
            foreach (TileBase adjacent in previouslyAdjacent)
                CalculateAdjacentTileCardTypes(adjacent);

            onTileSectionDestroyed?.Invoke(tile);
            return true;
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

        public List<Vector2Int> GetFreeTilePositions()
        {
            List<Vector2Int> returnList = new List<Vector2Int>();

            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    if (Tiles[x, y] == null)
                        returnList.Add(new Vector2Int(x, y));
                }
            }

            return returnList;
        }

        public bool TryGetRandomFreePosition(TileBase tile, out Vector2Int position, out float rotation)
        {
            List<Vector2Int> freePositions = GetFreeTilePositions();
            int breaker = 4 * GridSize.x * GridSize.y;

            rotation = 0f;
            position = Vector2Int.zero;

            while (breaker > 0)
            {
                rotation = Random.Range(0, 4) * 90f;
                position = freePositions.GetRandom();

                if (CanPlaceTile(tile, position, rotation))
                {
                    return true;
                }

                breaker--;
            }

            return false;
        }

        public bool TryPlaceTileRandomly(TileBase tile, bool invokeAddedTile = true)
        {
            List<Vector2Int> freePositions = GetFreeTilePositions();
            int breaker = 4 * GridSize.x * GridSize.y;

            while (breaker > 0)
            {
                float rotation = Random.Range(0, 4) * 90f;

                if (TryPlaceTile(tile, freePositions.GetRandom(), rotation, invokeAddedTile))
                {
                    return true;
                }

                breaker--;
            }

            return false;
        }

        public bool TryPlaceTileAnywhere(TileBase tile, Axis axis, bool invokeAddedTile = true)
        {
            switch (axis)
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

            CalculateSelfAndAdjacentTiles(selectedTile);

            if (invokeAddedTile)
                onTileAdded?.Invoke(selectedTile);
            return true;
        }

        private void CalculateSelfAndAdjacentTiles(TileBase tile)
        {
            CalculateAdjacentTileCardTypes(tile);
            List<TileBase> adjacentTiles = GetAdjacentTiles(tile);
            foreach (TileBase adjacentTile in adjacentTiles)
            {
                CalculateAdjacentTileCardTypes(adjacentTile);
            }
        }
        public void RecalculateAdjacentTileCardTypes(TileBase tile) => CalculateSelfAndAdjacentTiles(tile);
        public List<TileBase> GetAdjacentTiles(TileBase tile)
        {
            List<Vector2Int> checkedPoints = new List<Vector2Int>();
            List<TileBase> adjacentTiles = new List<TileBase>();
            foreach (Vector2Int position in tile.ActivePlacedShape)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        Vector2Int point = new Vector2Int(position.x + x, position.y + y);

                        if (!Tiles.IsInBounds(point.x, point.y))
                            continue;

                        if (checkedPoints.Contains(point))
                            continue;

                        checkedPoints.Add(point);
                        TileBase candidate = Tiles[point.x, point.y];

                        if (candidate == null || candidate == tile)
                            continue;

                        if (!adjacentTiles.Contains(candidate))
                            adjacentTiles.Add(candidate);
                    }
                }
            }

            return adjacentTiles;
        }

        public void CalculateAdjacentTileCardTypes(TileBase tile)
        {
            List<Vector2Int> checkedPoints = new List<Vector2Int>();
            tile.AdjacentTileTypes.Clear();
            foreach (Vector2Int position in tile.ActivePlacedShape)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        Vector2Int point = new Vector2Int(position.x + x, position.y + y);

                        if (!Tiles.IsInBounds(point.x, point.y))
                            continue;

                        if (checkedPoints.Contains(point))
                            continue;

                        checkedPoints.Add(point);
                        TileBase candidate = Tiles[point.x, point.y];

                        if (candidate == null || candidate == tile)
                            continue;

                        TileCardType flaggedTileCardType = candidate.TileCardType;

                        foreach (TileCardType tileCardType in flaggedTileCardType.CollapseFlags())
                        {
                            if (tile.AdjacentTileTypes.ContainsKey(tileCardType))
                            {
                                tile.AdjacentTileTypes[tileCardType]++;
                            }
                            else
                            {
                                tile.AdjacentTileTypes.Add(tileCardType, 1);
                            }
                        }
                    }
                }
            }
        }

        public bool RemoveTile(TileBase tile, bool invokeRemoveTile = true)
        {
            bool haveRemovedTile = false;

            List<TileBase> adjacentTiles = GetAdjacentTiles(tile);

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

                foreach (TileBase adjacentTile in adjacentTiles)
                {
                    CalculateAdjacentTileCardTypes(adjacentTile);
                }

                if (invokeRemoveTile)
                    onTileRemoved?.Invoke(tile);
            }

            return haveRemovedTile;
        }




    }

}