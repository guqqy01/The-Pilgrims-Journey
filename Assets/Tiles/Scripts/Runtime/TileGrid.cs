using System.Collections.Generic;
using UnityEngine;
using System;
using GameManagerCalls;

namespace Tiles
{
    public class TileGrid : MonoBehaviour, ISaveLoad, IOnSave, IOnLoadAfterMapPointSceneLoad, IOnClear
    {
        public string SaveFileName => $"TileGrid_{name}_Save";

        [SerializeField]
        protected Vector2Int gridSize;
        public Vector2Int GridSize => gridSize;

        protected Tile[,] tiles;
        public Tile[,] Tiles
        {
            get
            {
                if(tiles == null)
                    tiles = new Tile[gridSize.x, gridSize.y];

                return tiles;
            }
            set
            {
                tiles = value;
            }
        }

        public delegate void OnTileEvent(Tile tile);
        public OnTileEvent onTileAdded;
        public OnTileEvent onTileRemoved;
        public OnTileEvent onTileDestroyed;

        private void OnDestroy()
        {
            onTileAdded = null;
            onTileRemoved = null;
            onTileDestroyed = null;
        }

        public Tile this[int i, int j]
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

        public Tile this[Vector2Int position]
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

        public List<Tile> ListOfTiles
        {
            get
            {
                List<Tile> returnList = new List<Tile>();

                foreach (var tile in Tiles)
                {
                    if (tile != null && !returnList.Contains(tile))
                        returnList.Add(tile);
                }

                return returnList;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                Debug.Log($"Count: {ListOfTiles.Count}");
                foreach (var tile in ListOfTiles)
                {
                    Debug.Log($"Tile: {tile.TileSO.name}. Unit: {tile.UnitSO.name}");
                }
            }
        }

        public void Clear()
        {
            foreach (Tile tile in ListOfTiles)
            {
                DestroyTile(tile);
            }
            Tiles = new Tile[gridSize.x, gridSize.y];
        }

        public void DestroyTile(Tile tile)
        {
            tile.ActionDestroyTile -= DestroyTile;
            tile.OnDestroy();
            onTileDestroyed?.Invoke(tile);
        }

        public bool PointOutOfGrid(Vector2Int point) => point.x >= gridSize.x || point.x < 0 || point.y >= gridSize.y || point.y < 0;

        public bool CanPlaceTile(Tile tile, Vector2Int position, float rotation)
        {
            foreach (Vector2Int point in tile.Shape.Rotate(rotation))
            {
                if (PointOutOfGrid(point + position) ||
                    (TryGetTileAtPosition(point + position, out Tile outTile) && outTile != tile))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryGetTileAtPosition(Vector2Int v2Int, out Tile tile)
        {
            tile = null;

            if (PointOutOfGrid(v2Int))
                return false;

            tile = this[v2Int];

            return tile != null;
        }

        public bool TryPlaceTileAnywhere(Tile tile)
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

        public bool TryPlaceTile(Tile selectedTile, Vector2Int position, float rotation)
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

        public bool RemoveTile(Tile tile)
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

        public void PlaceTiles(List<TileAndUnit> tiles)
        {
            tiles.Sort();

            foreach (TileAndUnit tileAndUnit in tiles)
            {
                Tile tile = new Tile(tileAndUnit.tile, tileAndUnit.unit);
                TryPlaceTileAnywhere(tile);
            }
        }

        public List<TileSave> GetTileSaves()
        {
            List<TileSave> tileSaves = new List<TileSave>();

            foreach (Tile tile in ListOfTiles)
            {
                TileSave tileSave = new TileSave(tile.Level,
                    tile.CurrentAmountOfUnitsInTile + tile.AmountOfUnitsSpawned,
                    tile.CurrentAmountOfUnitsEscaped,
                    tile.Rotation,
                    tile.PlacedPosition,
                    tile.TileSO,
                    tile.UnitSO,
                    tile.Equipment);
                tileSaves.Add(tileSave);
            }

            return tileSaves;
        }

        public void PlaceTileSaves(List<TileSave> tileSaveData)
        {
            Clear();

            foreach (TileSave tileSave in tileSaveData)
            {
                if (PointOutOfGrid(tileSave.position))
                    continue;

                Tile tile = new Tile(tileSave.tile, tileSave.unit);

                if (!TryPlaceTile(tile, tileSave.position, tileSave.rotation))
                {
                    Debug.Log("Tile placement failed");
                }
            }
        }

        public void LoadAfterMapPointSceneLoad()
        {
            if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> saveProfile))
            {
                SaveData inventorySaveProfile = saveProfile.saveData;

                PlaceTileSaves(inventorySaveProfile.tileSaveDatas);
            }
            else
            {
                Debug.LogWarning($"In TileGrid {this}: There was no save file found, SaveFileName: {SaveFileName}");
            }
        }

        public void Save()
        {
            SaveData saveData = new SaveData(GetTileSaves());

            SaveProfile<SaveData> inventorySaveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);

            SaveManager.SaveOver(inventorySaveProfile);
        }

        [Serializable]
        private class SaveData : SaveProfileData
        {
            public List<TileSave> tileSaveDatas;

            public SaveData(List<TileSave> tileSaveDatas)
            {
                this.tileSaveDatas = tileSaveDatas;
            }
        }
    }

}

