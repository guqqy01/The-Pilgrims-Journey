using UnityEngine;
using System;
namespace Tiles
{
    public class AmountOfUnitTilesAllowedManager : Singleton<AmountOfUnitTilesAllowedManager>
    {
        [SerializeField]
        private TilePlayerTileGrid tilePlayerTileGrid;

        [SerializeField]
        private UnitExpandingTileGrid unitExpandingTileGrid;

        [SerializeField]
        [Range(0.5f, 1.5f)]
        private float amountOfUnitTilesAllowedMultiplier = 1f;
        private int _amountOfUnitTilesAllowed;
        public int AmountOfUnitTilesAllowed => _amountOfUnitTilesAllowed;
        public event Action<int> OnAmountOfUnitTilesAllowedChanged;

        [SerializeField]
        [Range(0f, 1f)]
        private float amountOfUnitTilesRequiredMultiplier = 0.5f;
        private int _amountOfUnitTilesRequired;
        public int AmountOfUnitTilesRequired => _amountOfUnitTilesRequired;
        public event Action<int> OnAmountOfUnitTilesRequiredChanged;

        [SerializeField]
        [Range(0.7f, 0.9f)]
        private float maxAmountOfUnitTilesRequiredMultiplier = 0.8f;

        private void Awake()
        {
            if (tilePlayerTileGrid != null)
            {
                tilePlayerTileGrid.onTileAdded += OnTileAdded;
                tilePlayerTileGrid.onTileRemoved += OnTileRemoved;
                tilePlayerTileGrid.onTileDestroyed += OnTileDestroyed;
            }

            if (unitExpandingTileGrid != null)
            {
                unitExpandingTileGrid.onTileAdded += OnTileAdded;
                unitExpandingTileGrid.onTileRemoved += OnTileRemoved;
                unitExpandingTileGrid.onTileDestroyed += OnTileDestroyed;
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

            if (unitExpandingTileGrid != null)
            {
                unitExpandingTileGrid.onTileAdded -= OnTileAdded;
                unitExpandingTileGrid.onTileRemoved -= OnTileRemoved;
                unitExpandingTileGrid.onTileDestroyed -= OnTileDestroyed;
            }
        }

        private void Start()
        {
            SetupAmountOfUnitTilesAllowed();
            SetupAmountOfUnitTilesRequired();
        }

        private void OnTileAdded(TileBase tileBase)
        {
            SetupAmountOfUnitTilesAllowed();
            SetupAmountOfUnitTilesRequired();
        }

        private void OnTileRemoved(TileBase tileBase)
        {
            SetupAmountOfUnitTilesAllowed();
            SetupAmountOfUnitTilesRequired();
        }

        private void OnTileDestroyed(TileBase tileBase)
        {
            SetupAmountOfUnitTilesAllowed();
            SetupAmountOfUnitTilesRequired();
        }

        private void SetupAmountOfUnitTilesAllowed()
        {
            int amountOfMonsterTiles = GetAmountOfMonsterTilesInTilePlayer();

            _amountOfUnitTilesAllowed = Mathf.FloorToInt(amountOfMonsterTiles * amountOfUnitTilesAllowedMultiplier);

            OnAmountOfUnitTilesAllowedChanged?.Invoke(_amountOfUnitTilesAllowed);
        }

        private void SetupAmountOfUnitTilesRequired()
        {
            int totalAmountOfUnitTiles = GetTotalAmountOfUnitTiles();

            int totalSpaceOnTilePlayer = tilePlayerTileGrid.GridSize.x * tilePlayerTileGrid.GridSize.y;
            
            int maxAmountOfUnits = Mathf.FloorToInt((totalSpaceOnTilePlayer / 2f) * maxAmountOfUnitTilesRequiredMultiplier);
            
            _amountOfUnitTilesRequired = Mathf.Min(Mathf.FloorToInt(totalAmountOfUnitTiles * amountOfUnitTilesRequiredMultiplier), maxAmountOfUnits);
            
            OnAmountOfUnitTilesRequiredChanged?.Invoke(_amountOfUnitTilesRequired);
        }

        public int GetAmountOfMonsterTilesInTilePlayer()
        {
            int amountOfMonsterTiles = 0;

            foreach (TileBase tile in tilePlayerTileGrid.ListOfTiles)
            {
                if (tile is MonsterTileBase)
                {
                    amountOfMonsterTiles += tile.Shape.Count;
                }
            }

            return amountOfMonsterTiles;
        }

        public int GetAmountOfUnitTilesInTilePlayer()
        {
            int amountOfUnitTiles = 0;

            foreach (TileBase tile in tilePlayerTileGrid.ListOfTiles)
            {
                if (tile is UnitTile)
                {
                    amountOfUnitTiles += tile.Shape.Count;
                }
            }

            return amountOfUnitTiles;
        }

        public int GetAmountOfUnitTilesInInventory()
        {
            int amountOfUnitTiles = 0;

            foreach (TileBase tile in unitExpandingTileGrid.ListOfTiles)
            {
                if (tile is UnitTile)
                {
                    amountOfUnitTiles += tile.Shape.Count;
                }
            }

            return amountOfUnitTiles;
        }

        public int GetTotalAmountOfUnitTiles()
        {
            return GetAmountOfUnitTilesInInventory() + GetAmountOfUnitTilesInTilePlayer();
        }
    }

}
