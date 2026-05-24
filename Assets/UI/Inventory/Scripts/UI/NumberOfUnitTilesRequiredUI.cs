using UnityEngine;
using Tiles;

namespace UI
{
    public class NumberOfUnitTilesRequiredUI : NumberOutOfNumberUI
    {
        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;
        [SerializeField]
        private AmountOfUnitTilesAllowedManager AmountOfUnitTilesAllowedManager;
        private void Awake()
        {
            if (AmountOfUnitTilesAllowedManager != null)
            {
                AmountOfUnitTilesAllowedManager.OnAmountOfUnitTilesRequiredChanged += OnRequiredAmountChanged;
            }
        }

        private void OnDestroy()
        {
            if (AmountOfUnitTilesAllowedManager != null)
            {
                AmountOfUnitTilesAllowedManager.OnAmountOfUnitTilesRequiredChanged -= OnRequiredAmountChanged;
            }
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

        private void OnRequiredAmountChanged(int requiredAmountChanged)
        {
            UpdateUI(GetAmountOfUnitTiles(), requiredAmountChanged);
        }
    }
}

