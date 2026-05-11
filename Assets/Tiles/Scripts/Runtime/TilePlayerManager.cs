using GameManagerCalls;
using System.Collections.Generic;
using Units;
using UnityEngine;
using Spells;
using LocalMap;
using System;

namespace Tiles
{

    public class TilePlayerManager : Singleton<TilePlayerManager>, IOnCompleteLocalMapLevel, IOnSetupNewGame, IOnClear
    {
        public float CurrentPlayingPosition { private set; get; }

        public delegate void OnCurrentPlayingPositionChange(Dictionary<UnitTile, int> spawningTilesAndSize);
        public OnCurrentPlayingPositionChange onPositionChangeNextSpawning;
        public OnCurrentPlayingPositionChange onPositionChangeCurrentSpawning;

        public delegate void NoRemainingTilesToPlay();
        public NoRemainingTilesToPlay noRemainingTilesToPlay;

        public delegate void OnPlay();
        public OnPlay onPlay;

        [SerializeField]
        private float oneTileTime = 15f;

        public bool IsPlaying { private set; get; } = false;

        public Dictionary<UnitTile, int> SpawningTilesAndSize { private set; get; } = new Dictionary<UnitTile, int>();
        public Dictionary<UnitTile, int> NextSpawningTilesAndSize { private set; get; } = new Dictionary<UnitTile, int>();

        [Header("Starting Tile")]
        [SerializeField]
        private TileSO startingTile;
        [SerializeField]
        private Vector2Int startTilePosition;
        [SerializeField]
        private float startTileRotation;
        [SerializeField]
        private UnitSO startingUnit;

        [SerializeField]
        private UnitTileGrid playerTileGrid;

        public Action OnStopPlaying;

        private void Awake()
        {
            if (playerTileGrid != null)
            {
                playerTileGrid.onTileAdded += OnTileAdded;
                playerTileGrid.onTileRemoved += OnTileRemoved;
            }
        }

        private void OnDestroy()
        {
            OnStopPlaying = null;
        }

        protected override void CleanUp()
        {
            onPositionChangeNextSpawning = null;
            onPositionChangeCurrentSpawning = null;
            noRemainingTilesToPlay = null;
            onPlay = null;

            if (playerTileGrid != null)
            {
                playerTileGrid.onTileAdded -= OnTileAdded;
                playerTileGrid.onTileRemoved -= OnTileRemoved;
            }
        }

        

        private void OnTileAdded(Tile<UnitSO> tile)
        {
            if (tile == null || tile.Item == null || tile.Item.unitsSpecialSpell == null)
                return;

            SpellManager.Instance.AddSpell(tile.Item.unitsSpecialSpell, tile.Shape.Count);
        }

        private void OnTileRemoved(Tile<UnitSO> tile)
        {
            if (tile == null || tile.Item == null || tile.Item.unitsSpecialSpell == null)
                return;

            SpellManager.Instance.RemoveSpell(tile.Item.unitsSpecialSpell, tile.Shape.Count);
        }

        private void Update()
        {
            if (!IsPlaying)
                return;

            int posBefore = Mathf.FloorToInt(CurrentPlayingPosition);

            CurrentPlayingPosition += Time.deltaTime / oneTileTime;

            
            if (posBefore != Mathf.FloorToInt(CurrentPlayingPosition))
            {
                PositionChanged();
            }
        }

        public void RemoveBlurOverride()
        {
            StopPlaying();

            if (playerTileGrid.PointOutOfGrid(startTilePosition))
            {
                Debug.LogWarning($"Starting tile position is out of the grid");
                return;
            }

            UnitTile tile = new UnitTile(startingTile, startingUnit);

            if(!playerTileGrid.TryPlaceTileAnywhere(tile))
            {
                Debug.LogWarning("Could not place tile.");
            }
        }


        public void Play()
        {
            if (PathPicker.Instance.PickedDirection == null)
                return;

            IsPlaying = true;

            PositionChanged();

            foreach (UnitTile tile in playerTileGrid.ListOfTiles)
            {
                tile.locked = true;
            }

            onPlay?.Invoke();
        }

        public void StopPlaying()
        {
            foreach (UnitTile tile in playerTileGrid.ListOfTiles)
            {
                tile.locked = false;
            }

            IsPlaying = false;
            CurrentPlayingPosition = 0;

            OnStopPlaying?.Invoke();
        }

        public void SetupNext()
        {

        }

        public void PositionChanged()
        {
            int newPosition = Mathf.FloorToInt(CurrentPlayingPosition);
            
            SpawningTilesAndSize = GetTilesInPositionAndSize(newPosition);
            NextSpawningTilesAndSize = GetTilesInPositionAndSize(newPosition + 1);

            onPositionChangeCurrentSpawning?.Invoke(SpawningTilesAndSize);
            onPositionChangeNextSpawning?.Invoke(NextSpawningTilesAndSize);

            if (!IsPlaying || SpawningTilesAndSize.Count > 0)
                return;

            List<UnitTile> remainingTiles = new List<UnitTile>();

            for (int i = newPosition + 1; i < playerTileGrid.GridSize.x + 1; i++)
            {
                remainingTiles.AddRange(GetTilesInPositionAndSize(i).Keys);
            }

            if (remainingTiles.Count == 0)
            {
                noRemainingTilesToPlay?.Invoke();
            }
        }

        public Dictionary<UnitTile, int> GetTilesInPositionAndSize(int position)
        {
            Dictionary<UnitTile, int> returnTiles = new Dictionary<UnitTile, int>();

            if (position >= playerTileGrid.Tiles.GetLength(0))
            {
                return returnTiles;
            }

            for (int i = 0; i < playerTileGrid.Tiles.GetLength(1); i++)
            {
                if (playerTileGrid.Tiles[position, i] != null)
                {
                    if (playerTileGrid.Tiles[position, i] is UnitTile unitTile)
                    if (returnTiles.ContainsKey(unitTile))
                    {
                        returnTiles[unitTile] += 1;
                    }
                    else
                    {
                        returnTiles.Add(unitTile, 1);
                    }
                }
            }

            return returnTiles;
        }

        public void OnCompleteLocalMapLevel()
        {
            StopPlaying();
        }

        public void Clear()
        {
            StopPlaying();
        }
    }
}


