using GameManagerCalls;
using UnityEngine;
using System.Collections.Generic;
using Monsters;
using GlobalMap;
using Codice.CM.Common;
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
        private MonsterExpandingTileGrid monsterExpandingTileGrid;
        [SerializeField]
        private UnitExpandingTileGrid unitExpandingTileGrid;
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



            foreach (MonsterTileBase monsterTile in monsterTiles)
            {
                if(currentamountOfTiles < amountOfMonsterTilesRequired)
                {
                    if(!tilePlayerTileGrid.TryPlaceTileAnywhere(monsterTile, TileGridBase.Axis.X))
                    {
                        if (!monsterExpandingTileGrid.TryPlaceTileAnywhere(monsterTile, TileGridBase.Axis.Y))
                        {
                            Debug.Log($"Not able to place monster tile with shape, {monsterTile.Shape} in monsterExpandingGrid");
                        }
                    }
                }
                else
                {
                    if(!monsterExpandingTileGrid.TryPlaceTileAnywhere(monsterTile, TileGridBase.Axis.Y))
                    {
                        Debug.Log($"Not able to place monster tile with shape, {monsterTile.Shape} in monsterExpandingGrid");
                    }
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
                monsterExpandingTileGrid.DestroyTile(monsterTile);
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
