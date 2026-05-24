using UnityEngine;
using Tiles;
using Helper.Wait;

namespace UI
{
    public class MonsterNumberOutOfNumberUI : NumberOutOfNumberUI
    {
        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;

        private void Awake()
        {
            if(tilePlayerTileGrid != null)
            {
                tilePlayerTileGrid.onTileAdded += OnTileAdded;
                tilePlayerTileGrid.onTileRemoved += OnTileRemoved;
                tilePlayerTileGrid.onTileDestroyed += OnTileDestroyed;
            }
        }

        private void Start()
        {
            WaitForHelper.WaitForObjectThenPerformAction(() => DifficultyLevelManager.Instance, UpdateMonsterDifficultyNumber);
        }

        private void OnDestroy()
        {
            if (tilePlayerTileGrid != null)
            {
                tilePlayerTileGrid.onTileAdded -= OnTileAdded;
                tilePlayerTileGrid.onTileRemoved -= OnTileRemoved;
                tilePlayerTileGrid.onTileDestroyed -= OnTileDestroyed;
            }
        }

        private void OnTileAdded(TileBase _)
        {
            UpdateMonsterDifficultyNumber();
        }

        private void OnTileRemoved(TileBase _)
        {
            UpdateMonsterDifficultyNumber();
        }

        private void OnTileDestroyed(TileBase _)
        {
            UpdateMonsterDifficultyNumber();
        }

        private void UpdateMonsterDifficultyNumber()
        {
            float difficultyValue = 0;

            foreach (TileBase tile in tilePlayerTileGrid.ListOfTiles)
            {
                if (tile is not MonsterTileBase monsterTile)
                    continue;

                difficultyValue += monsterTile.Difficulty;
            }

            UpdateUI(difficultyValue, DifficultyLevelManager.Instance.DifficultyLevel);
        }
    }
}

