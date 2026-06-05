using Helper.Wait;
using Tiles;
using UnityEngine;

namespace UI
{
    public class NumberOfUnitTilesRequiredUI : NumberOutOfNumberUI
    {
        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;
        [SerializeField]
        private AmountOfUnitTilesAllowedManager amountOfUnitTilesAllowedManager;
        private void Awake()
        {
            if (amountOfUnitTilesAllowedManager != null)
            {
                amountOfUnitTilesAllowedManager.OnAmountOfUnitTilesRequiredChanged += OnRequiredAmountChanged;
            }
        }

        private void Start()
        {
            WaitForHelper.WaitForObjectThenPerformAction(() => AmountOfUnitTilesAllowedManager.Instance, () => OnRequiredAmountChanged(amountOfUnitTilesAllowedManager.AmountOfUnitTilesRequired));
        }
        
        private void OnDestroy()
        {
            if (amountOfUnitTilesAllowedManager != null)
            {
                amountOfUnitTilesAllowedManager.OnAmountOfUnitTilesRequiredChanged -= OnRequiredAmountChanged;
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

