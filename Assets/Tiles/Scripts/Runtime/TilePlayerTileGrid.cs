using System.Collections.Generic;
using UnityEngine;
using System;
using GameManagerCalls;
using System.Linq;

namespace Tiles
{
    public class TilePlayerTileGrid : TileGridBase, ISaveLoad, IOnSave, IOnLoadAfterMapPointSceneLoad
    {
        public List<TileSave> GetTileSaves()
        {
            List<TileSave> tileSaves = new List<TileSave>();
            List<UnitTile> unitTiles = ListOfTiles.Where(x => x is UnitTile).Select(x => x as UnitTile).ToList();

            foreach (UnitTile tile in unitTiles)
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

                UnitTile tile = new UnitTile(tileSave.tile, tileSave.unit);

                if (!TryPlaceTile(tile, tileSave.position, tileSave.rotation))
                {
                    Debug.Log("Tile placement failed");
                }
            }
        }

        public void PlaceTiles(List<TileAndUnit> tiles)
        {
            tiles.Sort();

            foreach (TileAndUnit tileAndUnit in tiles)
            {
                UnitTile tile = new UnitTile(tileAndUnit.tile, tileAndUnit.unit);
                TryPlaceTileAnywhere(tile, Axis.X);
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

        public override bool AcceptTile(TileBase tile)
        {
            return tile is UnitTile || tile is MonsterTileBase;
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

