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
        [Tooltip("The multiplier for the amount of unit tiles allowed. depending on the number of monster tiles the player has, if the player has 10 monster tiles and this is 1.5f, the allowed would be 15")]
        private float amountOfUnitTilesAllowedMultiplier = 1f;
        private int _amountOfUnitTilesAllowed;
        public int AmountOfUnitTilesAllowed => _amountOfUnitTilesAllowed;
        public event Action<int> OnAmountOfUnitTilesAllowedChanged;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("The multiplier for the amount of unit tiles required. depending on the number of unit tiles the player has, if the player has 10 tiles the required would be 10 * this")]
        private float amountOfUnitTilesRequiredMultiplier = 0.5f;
        private int _amountOfUnitTilesRequired;
        public int AmountOfUnitTilesRequired => _amountOfUnitTilesRequired;
        public event Action<int> OnAmountOfUnitTilesRequiredChanged;

        [SerializeField]
        [Range(0.7f, 0.9f)]
        [Tooltip("The maximum amount of unit tiles required multiplier. " +
            "Where 1 would be half the tile player, and 0.5f would be a quarter.")]
        private float maxAmountOfUnitTilesRequiredMultiplier = 0.8f;

        protected override void Awake()
        {
            base.Awake();
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

        protected override void CleanUp()
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

        /// <summary>
        /// Sets up the amount of unit tiles allowed based on the amount of monster tiles in the tile player. 
        /// The more monster tiles there are, the more unit tiles are allowed. 
        /// This is to ensure that the player has enough unit tiles to counter the monster tiles, 
        /// but not too many that it becomes too easy.
        /// </summary>
        private void SetupAmountOfUnitTilesAllowed()
        {
            int amountOfMonsterTiles = GetAmountOfMonsterTilesInTilePlayer();

            _amountOfUnitTilesAllowed = Mathf.FloorToInt(amountOfMonsterTiles * amountOfUnitTilesAllowedMultiplier);

            OnAmountOfUnitTilesAllowedChanged?.Invoke(_amountOfUnitTilesAllowed);
        }

        /// <summary>
        /// Sets up the amount of unit tiles required based on the total amount of unit tiles.
        /// Maxed to half the space on the tile player multiplied by a multiplier to ensure that the player has enough unit tiles to fill the tile player,
        /// but not too many that it becomes too easy.
        /// </summary>
        private void SetupAmountOfUnitTilesRequired()
        {
            int minimumAmountOfUnitTilesRequired = int.MaxValue;

            foreach(TileBase tile in tilePlayerTileGrid.ListOfTiles)
            {
                if (tile is UnitTile)
                {
                    if(tile.Size < minimumAmountOfUnitTilesRequired)
                        minimumAmountOfUnitTilesRequired = tile.Size;
                }
            }

            foreach (TileBase tile in unitExpandingTileGrid.ListOfTiles)
            {
                if (tile is UnitTile)
                {
                    if (tile.Size < minimumAmountOfUnitTilesRequired)
                        minimumAmountOfUnitTilesRequired = tile.Size;
                }
            }

            int totalAmountOfUnitTiles = GetTotalAmountOfUnitTiles();

            int totalSpaceOnTilePlayer = tilePlayerTileGrid.GridSize.x * tilePlayerTileGrid.GridSize.y;
            
            int maxAmountOfUnits = Mathf.FloorToInt((totalSpaceOnTilePlayer / 2f) * maxAmountOfUnitTilesRequiredMultiplier);
            
            _amountOfUnitTilesRequired = Mathf.Max(Mathf.Min(Mathf.FloorToInt(totalAmountOfUnitTiles * amountOfUnitTilesRequiredMultiplier), maxAmountOfUnits), minimumAmountOfUnitTilesRequired);
            
            OnAmountOfUnitTilesRequiredChanged?.Invoke(_amountOfUnitTilesRequired);
        }

        public int SetupAndGetAmountOfUnitTilesRequired()
        {
            SetupAmountOfUnitTilesRequired();
            return AmountOfUnitTilesRequired;
        }

        public int GetAmountOfMonsterTilesInTilePlayer()
        {
            int amountOfMonsterTiles = 0;

            foreach (TileBase tile in tilePlayerTileGrid.ListOfTiles)
            {
                if (tile is MonsterTileBase)
                {
                    amountOfMonsterTiles += tile.Size;
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
                    amountOfUnitTiles += tile.Size;
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
                    amountOfUnitTiles += tile.Size;
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
