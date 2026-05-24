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
        private float difficultyAmountExtraMultiplier = 0.1f;

        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;
        [SerializeField]
        private MonsterExpandingTileGrid monsterExpandingTileGrid;

        private void SetupMonsterTiles()
        {
            MonsterSpawnSettings monsterSpawnSettings = GlobalMapGenerator.Instance.CurrentMapPoint.GenerationSettings.MonsterSpawnSettings;

            List<NormalMonsterSpawn> normalMonsterSpawns = monsterSpawnSettings.normalMonsterSpawns;
            List<WaveMonsterSpawn> waveMonsterSpawns = monsterSpawnSettings.waveMonsterSpawns;

            List<MonsterSpawn> monsterSpawns = new List<MonsterSpawn>();
            monsterSpawns.AddRange(normalMonsterSpawns);
            monsterSpawns.AddRange(waveMonsterSpawns);

            float difficultyNeeded = DifficultyLevelManager.Instance.DifficultyLevel * (1f + difficultyAmountExtraMultiplier);

            float currentDifficulty = 0;

            int breaker = 1000;

            while(currentDifficulty < difficultyNeeded && breaker > 0)
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
                currentDifficulty += monsterTile.Difficulty;
            }

            currentDifficulty = 0;

            foreach (MonsterTileBase monsterTile in monsterTiles)
            {
                if(currentDifficulty < difficultyNeeded)
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
