using UnityEngine;
using System;
namespace Tiles
{
    public class AmountOfUnitTilesAllowedManager : Singleton<AmountOfUnitTilesAllowedManager>
    {
        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;

        [SerializeField]
        [Range(0.5f, 1.5f)]
        private float amountOfUnitTilesMultiplier = 1f;
        private int amountOfUnitTilesAllowed;
        public int AmountOfUnitTilesAllowed => amountOfUnitTilesAllowed;
        public event Action<int> OnAmountOfUnitTilesAllowedChanged;

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

        private void OnTileAdded(TileBase tileBase)
        {
            SetupAmountOfUnitTilesAllowed();
        }

        private void OnTileRemoved(TileBase tileBase)
        {
            SetupAmountOfUnitTilesAllowed();
        }

        private void OnTileDestroyed(TileBase tileBase)
        {
            SetupAmountOfUnitTilesAllowed();
        }

        private void SetupAmountOfUnitTilesAllowed()
        {
            int amountOfMonsterTiles = 0;

            foreach (TileBase tile in tilePlayerTileGrid.ListOfTiles)
            {
                if (tile is MonsterTileBase)
                {
                    amountOfMonsterTiles += tile.Shape.Count;
                }
            }

            amountOfUnitTilesAllowed = Mathf.FloorToInt(amountOfMonsterTiles * amountOfUnitTilesMultiplier);

            OnAmountOfUnitTilesAllowedChanged?.Invoke(amountOfUnitTilesAllowed);
        }
    }

}
