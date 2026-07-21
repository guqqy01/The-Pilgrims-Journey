using UnityEngine;
using Tiles;
using Helper.Wait;

namespace UI
{

    public class NumberOfUnitTilesAllowedUI : NumberOutOfNumberUI
    {
        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;
        [SerializeField]
        private AmountOfUnitTilesAllowedManager amountOfUnitTilesAllowedManager;
        private void Awake()
        {
            if (amountOfUnitTilesAllowedManager != null)
            {
                tilePlayerTileGrid.onTileAdded += OnTileAdded;
                tilePlayerTileGrid.onTileRemoved += OnTileRemoved;
                tilePlayerTileGrid.onTileDestroyed += OnTileDestroyed;
            }

            if (amountOfUnitTilesAllowedManager != null)
            {
                amountOfUnitTilesAllowedManager.OnAmountOfUnitTilesAllowedChanged += OnAllowedAmountChanged;
            }
        }

        private void Start()
        {
            WaitForHelper.WaitForObjectThenPerformAction(() => AmountOfUnitTilesAllowedManager.Instance, () => OnAllowedAmountChanged(amountOfUnitTilesAllowedManager.AmountOfUnitTilesAllowed));
        }

        private void OnDestroy()
        {
            if (tilePlayerTileGrid != null)
            {
                tilePlayerTileGrid.onTileAdded -= OnTileAdded;
                tilePlayerTileGrid.onTileRemoved -= OnTileRemoved;
                tilePlayerTileGrid.onTileDestroyed -= OnTileDestroyed;
            }

            if (amountOfUnitTilesAllowedManager != null)
            {
                amountOfUnitTilesAllowedManager.OnAmountOfUnitTilesAllowedChanged -= OnAllowedAmountChanged;
            }
        }

        private void OnTileAdded(TileBase _)
        {
           UpdateUI(GetAmountOfUnitTiles(), amountOfUnitTilesAllowedManager.AmountOfUnitTilesAllowed);
        }

        private void OnTileRemoved(TileBase _)
        {
            UpdateUI(GetAmountOfUnitTiles(), amountOfUnitTilesAllowedManager.AmountOfUnitTilesAllowed);
        }

        private void OnTileDestroyed(TileBase _)
        {
            UpdateUI(GetAmountOfUnitTiles(), amountOfUnitTilesAllowedManager.AmountOfUnitTilesAllowed);
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

