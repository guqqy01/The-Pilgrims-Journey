using System.Collections.Generic;
using UnityEngine;
using System;
namespace Tiles
{

    public class ExpandingTileGrid<TItem> : TileGrid<TItem>
    {
        public Axis ExpandingDirection;
        public int otherSideGridLength;

        [SerializeField]
        private int minExpandingSize = 10;
        [SerializeField]
        private int minExtraSize = 3;

        [SerializeField]
        private int maxExtraOverLength = 3;

        public event Action onGridResized;

        protected virtual void Awake()
        {
            SetupSize();
            onTileAdded += OnTileAdded;
            onTileRemoved += OnTileRemoved;
            onTileDestroyed += OnTileDestroyed;
        }

        protected virtual void OnDestroy()
        {
            onTileAdded = null;
            onTileRemoved = null;
            onTileDestroyed = null;
        }

        private void OnTileAdded(Tile<TItem> tile)
        {
            SetupSize();
        }

        private void OnTileRemoved(Tile<TItem> tile)
        {
            SetupSize();
        }

        private void OnTileDestroyed(Tile<TItem> tile)
        {
            SetupSize();
        }

        private void SetupSize()
        {
            List<Tile<TItem>> tiles = ListOfTiles;
            int expandingSideLength = Mathf.Max(minExpandingSize, Mathf.Min(GetFurthestPlacedPosition(tiles) + minExtraSize, GetMaxSize(tiles)));

            Vector2Int newSize = gridSize;

            switch (ExpandingDirection)
            {
                case Axis.X:
                    newSize = new Vector2Int(expandingSideLength, otherSideGridLength);
                    break;
                case Axis.Y:
                    newSize = new Vector2Int(otherSideGridLength, expandingSideLength);
                    break;
                case Axis.MinusX:
                    newSize = new Vector2Int(expandingSideLength, otherSideGridLength);
                    break;
                case Axis.MinusY:
                    newSize = new Vector2Int(otherSideGridLength, expandingSideLength);
                    break;
            }

            if (newSize != gridSize)
            {
                ResizeTiles(newSize);
            }
        }

        private int GetMaxSize(List<Tile<TItem>> tiles)
        {
            int maxBounds = 0;

            foreach (var tile in tiles)
            {
                maxBounds += tile.RotatedShape.Bounds().Max();
            }

            return maxBounds + maxExtraOverLength;
        }

        private int GetFurthestPlacedPosition(List<Tile<TItem>> tiles)
        {
            int furthest = 0;

            if(ExpandingDirection == Axis.X || ExpandingDirection == Axis.MinusX)
            {
                foreach (var tile in tiles)
                    foreach (var point in tile.RotatedPlacedShape)
                        if (point.x > furthest)
                            furthest = point.x;
            }
            else
            {
                foreach (var tile in tiles)
                    foreach (var point in tile.RotatedPlacedShape)
                        if (point.y > furthest)
                            furthest = point.y;
            }

            return furthest;
        }

        public override bool TryPlaceTile(Tile<TItem> selectedTile, Vector2Int position, float rotation, bool invokeAddedTile = true, bool invokeRemoveTile = true)
        {
            if(TileOutOfGrid(selectedTile, position, rotation))
            {
                SetupSize();
            }
            return base.TryPlaceTile(selectedTile, position, rotation, invokeAddedTile);
        }

        private void ResizeTiles(Vector2Int newSize)
        {
            var oldTiles = Tiles;
            tiles = new Tile<TItem>[newSize.x, newSize.y];
            // copy over existing tiles that still fit
            for (int x = 0; x < Mathf.Min(oldTiles.GetLength(0), newSize.x); x++)
                for (int y = 0; y < Mathf.Min(oldTiles.GetLength(1), newSize.y); y++)
                    tiles[x, y] = oldTiles[x, y];
            gridSize = newSize;
            onGridResized?.Invoke();
        }
    }

}

