using UnityEngine;
using Tiles;
using Helper.Wait;

namespace UI
{
    public class UnitNumberOutOfNumberUI : NumberOutOfNumberUI
    {
        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;
        [SerializeField]
        private AmountOfUnitTilesAllowedManager AmountOfUnitTilesAllowedManager;
        private void Awake()
        {
            if (AmountOfUnitTilesAllowedManager != null)
            {
                tilePlayerTileGrid.onTileAdded += OnTileAdded;
                tilePlayerTileGrid.onTileRemoved += OnTileRemoved;
                tilePlayerTileGrid.onTileDestroyed += OnTileDestroyed;
            }

            if (AmountOfUnitTilesAllowedManager != null)
            {
                AmountOfUnitTilesAllowedManager.OnAmountOfUnitTilesAllowedChanged += OnAllowedAmountChanged;
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

            if (AmountOfUnitTilesAllowedManager != null)
            {
                AmountOfUnitTilesAllowedManager.OnAmountOfUnitTilesAllowedChanged -= OnAllowedAmountChanged;
            }
        }

        private void OnTileAdded(TileBase _)
        {
           UpdateUI(GetAmountOfUnitTiles(), AmountOfUnitTilesAllowedManager.AmountOfUnitTilesAllowed);
        }

        private void OnTileRemoved(TileBase _)
        {
            UpdateUI(GetAmountOfUnitTiles(), AmountOfUnitTilesAllowedManager.AmountOfUnitTilesAllowed);
        }

        private void OnTileDestroyed(TileBase _)
        {
            UpdateUI(GetAmountOfUnitTiles(), AmountOfUnitTilesAllowedManager.AmountOfUnitTilesAllowed);
        }

        private int GetAmountOfUnitTiles()
        {
            int amountOfUnitTiles = 0;  

            foreach (TileBase tile in tilePlayerTileGrid.ListOfTiles)
            {
                if (tile is UnitTile unitTile)
                {
                    amountOfUnitTiles += tile.Shape.Count;
                }
            }

            return amountOfUnitTiles;
        }

        private void OnAllowedAmountChanged(int allowedAmountChanged)
        {
            UpdateUI(GetAmountOfUnitTiles(), allowedAmountChanged);
        }
    }
}

