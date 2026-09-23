using GameManagerCalls;
using GlobalMap;
using Monsters;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Tiles
{

    public class MonsterTileManager : Singleton<MonsterTileManager>, IOnCreateLocalMap
    {
        [SerializeField]
        private WeightedChanceList<TileSO> tiles;

        public List<MonsterTileBase> monsterTiles = new List<MonsterTileBase>();

        [SerializeField]
        [Range(0f, 1f)]
        private float amountOfMonsterTilesExtraMultiplier = 0.1f;

        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;
        
        [SerializeField]
        private UnitExpandingTileGrid unitExpandingTileGrid;

        public MonsterTileBase GetMonsterTile(TileSO tile, MonsterSpawn monster)
        {
            if(monster is NormalMonsterSpawn normal)
            {
                return new NormalMonsterTile(tile, normal);
            }
            else if(monster is WaveMonsterSpawn wave)
            {
                return new WaveMonsterTile(tile, wave);
            }

            return null;
        }

        public List<MonsterTileBase> GetMonsterTiles(MonsterSpawnSettings monsterSpawnSettings, List<TileSO> tiles, int amountOfTiles)
        {
            List<MonsterTileBase> returnMonsterTiles = new List<MonsterTileBase>();

            List<NormalMonsterSpawn> normalMonsterSpawns = monsterSpawnSettings.normalMonsterSpawns;
            List<WaveMonsterSpawn> waveMonsterSpawns = monsterSpawnSettings.waveMonsterSpawns;

            List<MonsterSpawn> monsterSpawns = new List<MonsterSpawn>();
            monsterSpawns.AddRange(normalMonsterSpawns);
            monsterSpawns.AddRange(waveMonsterSpawns);

            for (int i = 0; i < amountOfTiles; i++)
            {
                MonsterSpawn monsterSpawn = monsterSpawns.GetRandom();
                MonsterTileBase monsterTile = GetMonsterTile(tiles.GetRandom(), monsterSpawn);

                if (monsterTile == null)
                    continue;

                returnMonsterTiles.Add(monsterTile);
            }

            return returnMonsterTiles;
        }

        private void SetupMonsterTiles()
        {
            MonsterSpawnSettings monsterSpawnSettings = GlobalMapGenerator.Instance.CurrentMapPoint.GenerationSettings.MonsterSpawnSettings;

            List<NormalMonsterSpawn> normalMonsterSpawns = monsterSpawnSettings.normalMonsterSpawns;
            List<WaveMonsterSpawn> waveMonsterSpawns = monsterSpawnSettings.waveMonsterSpawns;

            List<MonsterSpawn> monsterSpawns = new List<MonsterSpawn>();
            monsterSpawns.AddRange(normalMonsterSpawns);
            monsterSpawns.AddRange(waveMonsterSpawns);

            int amountOfMonsterTilesRequired =  GetTheAmountOfMonsterTilesRequired();

            int maxAmountOfMonsterTileAvaliable = Mathf.Max(amountOfMonsterTilesRequired, Mathf.RoundToInt(amountOfMonsterTilesRequired * (1f + amountOfMonsterTilesExtraMultiplier)));

            float currentamountOfTiles = 0;

            int breaker = 1000;

            while(currentamountOfTiles < maxAmountOfMonsterTileAvaliable && breaker > 0)
            {
                breaker--;

                MonsterSpawn monsterSpawn = monsterSpawns.GetRandom();
                MonsterTileBase monsterTile = null;

                if(monsterSpawn is NormalMonsterSpawn normalMonsterSpawn)
                {
                    monsterTile = new NormalMonsterTile(tiles.GetRandom(), normalMonsterSpawn);
                }
                else if(monsterSpawn is WaveMonsterSpawn waveMonsterSpawn)
                {
                    monsterTile = new WaveMonsterTile(tiles.GetRandom(), waveMonsterSpawn);
                }

                if (monsterTile == null)
                    continue;

                monsterTiles.Add(monsterTile);
                currentamountOfTiles += monsterTile.Size;
            }

            currentamountOfTiles = 0;

            List<UnitTile> unitTiles = tilePlayerTileGrid.ListOfTiles.Where(x => x is UnitTile).Select(x => x as UnitTile).ToList();

            foreach(UnitTile unitTile in unitTiles)
            {
                tilePlayerTileGrid.RemoveTile(unitTile, false);
            }

            foreach (MonsterTileBase monsterTile in monsterTiles)
            {
                if (currentamountOfTiles < amountOfMonsterTilesRequired)
                {
                    if (tilePlayerTileGrid.TryPlaceTileRandomly(monsterTile))
                    {
                        monsterTile.Locked = true;
                    }
                }
            }

            foreach (UnitTile unitTile in unitTiles)
            {
                if (tilePlayerTileGrid.TryPlaceTileAnywhere(unitTile, TileGridBase.Axis.X))
                    continue; // same grid — existing TileGridUI.OnTileAdded reuse logic already handles this fine

                if (unitExpandingTileGrid.TryPlaceTileAnywhere(unitTile, TileGridBase.Axis.Y, invokeAddedTile: false))
                {
                    TileGridTransferEvents.Raise(unitTile, tilePlayerTileGrid, unitExpandingTileGrid);
                }
                else
                {
                    Debug.LogWarning($"No home found for {unitTile} after monster tile setup.");
                    TileGridTransferEvents.Raise(unitTile, tilePlayerTileGrid, null);
                }
            }
        }

        private int GetTheAmountOfMonsterTilesRequired()
        {
            return AmountOfUnitTilesAllowedManager.Instance.SetupAndGetAmountOfUnitTilesRequired();
        }

        public void Clear()
        {
            foreach(MonsterTileBase monsterTile in monsterTiles)
            {
                tilePlayerTileGrid.DestroyTile(monsterTile);
            }

            monsterTiles.Clear();
        }

        public void OnCreateLocalMap()
        {
            Clear();
            SetupMonsterTiles();
        }
    }

}
