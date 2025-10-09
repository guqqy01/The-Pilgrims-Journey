using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Equipment;
using Units;
using UnityEngine.UI;

namespace Tiles
{
    public class TileGrid : MonoBehaviour, ISaveLoad
    {
        public string SaveFileName => $"TileGrid_{name}_{GetHashCode()}_Save";

        [SerializeField]
        private RectTransform gridTransform;
        public RectTransform GridTransform => gridTransform;

        protected Tile[,] tiles;
        public Tile[,] Tiles
        {
            get
            {
                if (tiles == null)
                    tiles = new Tile[gridSize.x, gridSize.y];

                return tiles;
            }
            set
            {
                tiles = value;
            }
        }

        //One pixel width, 32 pixels per unit
        private static Vector2 SizeDeltaOffset = new Vector2(-3.125f, -3.125f);

        public float TileSizeX => (GridTransform.sizeDelta.x + SizeDeltaOffset.x) / GridSize.x;
        public float TileSizeY => (GridTransform.sizeDelta.y + SizeDeltaOffset.y) / GridSize.y;

        public Vector2 TileSize => new Vector2(TileSizeX, TileSizeY);

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

        public delegate void OnTileChange();
        public OnTileChange onTileChange;

        public delegate void OnTileAdded(Tile tile);
        public OnTileAdded onTileAdded;

        public delegate void OnTileRemoved(Tile tile);
        public OnTileRemoved onTileRemoved;

        [SerializeField]
        protected Vector2Int gridSize;
        public Vector2Int GridSize => gridSize;

        private Image gridImageForValidation;
        protected virtual void Awake()
        {
            GameManager.onSave += Save;
            GameManager.onLoad += Load;
            GameManager.onClear += Clear;
        }

        private void OnValidate()
        {
            if (gridImageForValidation == null)
                gridImageForValidation = gridTransform.GetComponent<Image>();

            if (gridImageForValidation == null || gridImageForValidation.sprite == null)
                return;


            gridTransform.sizeDelta = (100f / gridImageForValidation.sprite.pixelsPerUnit) * (14f * ((Vector2)gridSize) + Vector2.one);
        }

        public void Clear()
        {
            foreach (var tile in Tiles)
            {
                if (tile != null)
                    Destroy(tile.gameObject);
            }

            Tiles = new Tile[gridSize.x, gridSize.y];
        }

        

        public bool PointOutOfGrid(Vector2Int point) => point.x >= gridSize.x || point.x < 0 || point.y >= gridSize.y || point.y < 0;

        public bool CanPlaceTile(Tile tile, Vector2Int position)
        {
            foreach (Vector2Int point in tile.Shape)
            {
                if (PointOutOfGrid(point + position) ||
                    (TryGetTileAtPosition(point + position, out Tile outTile) && outTile != tile))
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanPlaceTileFromWorldPosition(Tile tile, Vector2 worldPosition)
        {
            return CanPlaceTile(tile, GetGridPositon(worldPosition));
        }

        public bool TryPlaceTile(Tile selectedTile, Vector2Int position)
        {
            if (!CanPlaceTile(selectedTile, position))
                return false;

            RemoveTile(selectedTile);

            foreach (var point in selectedTile.Shape.AddToAll(position))
            {
                Tiles[point.x, point.y] = selectedTile;
            }

            selectedTile.PlacedPosition = position;
            selectedTile.PlacedRotation = selectedTile.Rotation;
            selectedTile.RectTransform.position = Position(selectedTile, position);
            selectedTile.tileGrid = this;
            selectedTile.transform.SetParent(GridTransform, true);

            onTileChange?.Invoke();
            return true;
        }

        public bool TryPlaceTileFromWorldPosition(Tile tile, Vector2 worldPosition)
        {
            return TryPlaceTile(tile, GetGridPositon(worldPosition));
        }

        public Vector2 Position(Tile tile, Vector2Int position)
        {
            return (Vector2)GridTransform.position + TileUIManager.Instance.canvas.scaleFactor * (TileSize * (position - ((Vector2)GridSize - Vector2.one) / 2f) + tile.GetOffset() * TileSize);
        }


        public bool TryGetTileAtMousePosition(out Tile tile)
        {
            return TryGetTileAtPosition(GetMouseGridPosition(), out tile);
        }

        public bool TryGetTileAtPosition(Vector2Int v2Int, out Tile tile)
        {
            tile = null;

            if (PointOutOfGrid(v2Int))
                return false;

            tile = this[v2Int];

            return tile != null;
        }

        public Vector2Int GetGridPositon(Vector2 worldPosition)
        {
            Vector2 relativePos = (worldPosition - (Vector2)GridTransform.position) / GridTransform.lossyScale.x + (GridTransform.sizeDelta + SizeDeltaOffset) / 2f;
            return new Vector2Int(Mathf.CeilToInt(relativePos.x / TileSizeX), Mathf.CeilToInt(relativePos.y / TileSizeY)) - Vector2Int.one;
        }

        public Vector2Int GetMouseGridPosition()
        {
            return GetGridPositon(Input.mousePosition);
        }

        public bool IsWorldPositionInGrid(Vector2 position)
        {
            return !PointOutOfGrid(GetGridPositon(position));
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

            if (haveRemovedTile)
                onTileChange?.Invoke();

            return haveRemovedTile;
        }

        public void Save()
        {
            SaveData saveData = new SaveData(GetTileSaves());

            SaveProfile<SaveData> inventorySaveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);

            SaveManager.SaveOver(inventorySaveProfile);
        }

        public void PlaceTiles(List<TileAndUnit> tiles)
        {
            tiles.Sort();
            
            List<Tile> instantiatedTiles = new List<Tile>();

            foreach (TileAndUnit tileAndUnit in tiles)
            {
                TileSO tileSO = tileAndUnit.tile;
                Tile tile = Instantiate(tileSO.tilePrefab, GridTransform);
                tile.BaseInit(tileSO, tileAndUnit.unit);
                instantiatedTiles.Add(tile);
            }

            foreach (Tile tile in instantiatedTiles)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int x = 0; x < gridSize.x; x++)
                    {
                        for (float rotation = 0; rotation < 360; rotation += 90)
                        {
                            tile.RotateTo(rotation);

                            if (TryPlaceTile(tile, new Vector2Int(x, y)))
                            {
                                goto NextTile;
                            }
                        }
                    }
                }

                Destroy(tile.gameObject);

                NextTile:;
            }
        }

        public List<TileSave> GetTileSaves()
        {
            List<TileSave> tileSaves = new List<TileSave>();

            foreach (Tile tile in ListOfTiles)
            {
                TileSave tileSave = new TileSave(tile.Level, tile.Rotation, tile.PlacedPosition, tile.TileSO, tile.UnitSO, tile.Equipment);
                tileSaves.Add(tileSave);
            }

            return tileSaves;
        }

        public void Load()
        {
            if(SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> saveProfile))
            {
                SaveData inventorySaveProfile = saveProfile.saveData;

                PlaceTileSaves(inventorySaveProfile.tileSaveDatas);
            }
        }

        public void PlaceTileSaves(List<TileSave> tileSaveData)
        {
            Clear();

            foreach (TileSave tileSave in tileSaveData)
            {
                TileSO tileSO = tileSave.tile;
                Vector2Int position = tileSave.position;

                if (PointOutOfGrid(position))
                    return;

                Tile tile = Instantiate(tileSO.tilePrefab, GridTransform);
                tile.TileSaveInit(tileSave);

                TryPlaceTile(tile, position);
            }
        }

        [Serializable]
        private record SaveData : SaveProfileData
        {
            public List<TileSave> tileSaveDatas;

            public SaveData(List<TileSave> tileSaveDatas)
            {
                this.tileSaveDatas = tileSaveDatas;
            }
        }
    }

    [Serializable]
    public class TileSave
    {
        public int level;
        public float rotation;
        public Vector2Int position;
        public TileSO tile;
        public UnitSO unit;
        public EquipmentSO[] equipment;

        public TileSave(int level, float rotation, Vector2Int position, TileSO tile, UnitSO unit, EquipmentSO[] equipment)
        {
            this.level = level;
            this.rotation = rotation;
            this.position = position;
            this.tile = tile;
            this.unit = unit;
            this.equipment = equipment;
        }

        public TileAndUnit TileAndUnit => new TileAndUnit(tile, unit);
    }

}

