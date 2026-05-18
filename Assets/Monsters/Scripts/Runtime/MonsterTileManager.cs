using GameManagerCalls;
using UnityEngine;
using System.Collections.Generic;
using Monsters;
using LocalMap;
using GlobalMap;

namespace Tiles
{
    public class MonsterTileManager : Singleton<MonsterTileManager>, IOnCreateLocalMap
    {
        [SerializeField]
        private WeightedChanceList<TileSO> tiles;

        public List<MonsterTile> monsterTiles = new List<MonsterTile>();

        [SerializeField]
        private float difficultyAmountExtraMultiplier = 0.1f;

        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;
        [SerializeField]
        private MonsterExpandingTileGrid monsterExpandingTileGrid;

        private void SetupMonsterTiles()
        {
            MonsterSpawnSettings monsterSpawnSettings = GlobalMapGenerator.Instance.CurrentMapPoint.GenerationSettings.MonsterSpawnSettings;

            List<MonsterSpawn> monsterSpawns = monsterSpawnSettings.monsterSpawns;

            float difficultyNeeded = DifficultyLevelManager.Instance.DifficultyLevel * (1f + difficultyAmountExtraMultiplier);

            float currentDifficulty = 0;

            while(currentDifficulty < difficultyNeeded)
            {
                MonsterTile monsterTile = new MonsterTile(tiles.GetRandom(), monsterSpawns.GetRandom());
                monsterTiles.Add(monsterTile);
                currentDifficulty += monsterTile.Difficulty;
            }

            currentDifficulty = 0;

            foreach (MonsterTile monsterTile in monsterTiles)
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
            foreach(MonsterTile monsterTile in monsterTiles)
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
