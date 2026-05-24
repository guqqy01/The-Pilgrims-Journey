using UnityEngine;
using Tiles;

namespace UI
{
    public class UnitNumberOutOfNumberUI : NumberOutOfNumberUI
    {
        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;

        private void Awake()
        {
            if (tilePlayerTileGrid != null)
            {
                tilePlayerTileGrid.onTileAdded += OnTileAdded;
                tilePlayerTileGrid.onTileRemoved += OnTileRemoved;
                tilePlayerTileGrid.onTileDestroyed += OnTileDestroyed;
            }

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
            UpdateRequiredUnitsNumber();
        }

        private void OnTileRemoved(TileBase _)
        {
            UpdateRequiredUnitsNumber();
        }

        private void OnTileDestroyed(TileBase _)
        {
            UpdateRequiredUnitsNumber();
        }

        private void UpdateRequiredUnitsNumber()
        {
            float difficultyValue = 0;

            foreach (TileBase tile in tilePlayerTileGrid.ListOfTiles)
            {
                if (tile is not UnitTile unitTile)
                    continue;
            }
        }
    }
}

