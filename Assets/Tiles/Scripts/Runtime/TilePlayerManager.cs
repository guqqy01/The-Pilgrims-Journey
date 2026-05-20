using GameManagerCalls;
using LocalMap;
using Spells;
using System;
using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Tiles
{
    public class TilePlayerManager : Singleton<TilePlayerManager>, IOnCompleteLocalMapLevel, IOnSetupNewGame, IOnClear
    {
        public float CurrentPlayingPosition { private set; get; }

        public delegate void OnPositionChangeUnitTiles(Dictionary<UnitTile, int> spawningTilesAndSize);
        public event OnPositionChangeUnitTiles onPositionChangeNextUnitTiles;
        public event OnPositionChangeUnitTiles onPositionChangeCurrentUnitTiles;

        public delegate void OnPositionChangeMonsterTiles(Dictionary<MonsterTile, int> spawningTilesAndSize);
        public event OnPositionChangeMonsterTiles onPositionChangeNextMonsterTiles;
        public event OnPositionChangeMonsterTiles onPositionChangeCurrentMonsterTiles;

        public delegate void NoRemainingTilesToPlay();
        public NoRemainingTilesToPlay noRemainingTilesToPlay;

        public delegate void OnPlay();
        public OnPlay onPlay;

        [SerializeField]
        private float oneTileTime = 15f;

        public bool IsPlaying { private set; get; } = false;

        public Dictionary<UnitTile, int> SpawningUnitTilesAndSize { private set; get; } = new Dictionary<UnitTile, int>();
        public Dictionary<UnitTile, int> NextSpawningUnitTilesAndSize { private set; get; } = new Dictionary<UnitTile, int>();

        public Dictionary<MonsterTile, int> SpawningMonsterTilesAndSize { private set; get; } = new Dictionary<MonsterTile, int>();
        public Dictionary<MonsterTile, int> NextSpawningMonsterTilesAndSize { private set; get; } = new Dictionary<MonsterTile, int>();

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
        private TilePlayerTileGrid playerTileGrid;

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
            onPositionChangeNextUnitTiles = null;
            onPositionChangeCurrentUnitTiles = null;
            noRemainingTilesToPlay = null;
            onPlay = null;

            onPositionChangeNextMonsterTiles = null;
            onPositionChangeCurrentMonsterTiles = null;

            if (playerTileGrid != null)
            {
                playerTileGrid.onTileAdded -= OnTileAdded;
                playerTileGrid.onTileRemoved -= OnTileRemoved;
            }
        }

        private void OnTileAdded(TileBase tile)
        {
            if (tile == null)
                return;

            if (tile is not UnitTile unitTile) 
                return;

            if (unitTile.UnitSO == null || unitTile.UnitSO.unitsSpecialSpell == null)
                return;

            SpellManager.Instance.AddSpell(unitTile.UnitSO.unitsSpecialSpell, tile.Shape.Count);
        }

        private void OnTileRemoved(TileBase tile)
        {
            if (tile == null)
                return;

            if (tile is not UnitTile unitTile)
                return;

            if (unitTile.UnitSO == null || unitTile.UnitSO.unitsSpecialSpell == null)
                return;

            SpellManager.Instance.RemoveSpell(unitTile.UnitSO.unitsSpecialSpell, tile.Shape.Count);
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

        public void OnSetupNewGame()
        {
            StopPlaying();

            if (playerTileGrid.PointOutOfGrid(startTilePosition))
            {
                Debug.LogWarning($"Starting tile position is out of the grid");
                return;
            }

            UnitTile tile = new UnitTile(startingTile, startingUnit);

            if(!playerTileGrid.TryPlaceTileAnywhere(tile, TileGridBase.Axis.X))
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

            foreach (TileBase tile in playerTileGrid.ListOfTiles)
            {
                tile.locked = true;
            }

            onPlay?.Invoke();
        }

        public void StopPlaying()
        {
            foreach (TileBase tile in playerTileGrid.ListOfTiles)
            {
                tile.locked = false;
            }

            IsPlaying = false;
            CurrentPlayingPosition = 0;

            OnStopPlaying?.Invoke();
        }

        public void PositionChanged()
        {
            int newPosition = Mathf.FloorToInt(CurrentPlayingPosition);
            
            SpawningUnitTilesAndSize = GetUnitTilesInPositionAndSize(newPosition);
            NextSpawningUnitTilesAndSize = GetUnitTilesInPositionAndSize(newPosition + 1);

            SpawningMonsterTilesAndSize = GetMonsterTilesInPositionAndSize(newPosition);
            NextSpawningMonsterTilesAndSize = GetMonsterTilesInPositionAndSize(newPosition + 1);

            onPositionChangeCurrentUnitTiles?.Invoke(SpawningUnitTilesAndSize);
            onPositionChangeNextUnitTiles?.Invoke(NextSpawningUnitTilesAndSize);

            onPositionChangeCurrentMonsterTiles?.Invoke(SpawningMonsterTilesAndSize);
            onPositionChangeNextMonsterTiles?.Invoke(NextSpawningMonsterTilesAndSize);

            if (!IsPlaying || SpawningUnitTilesAndSize.Count > 0)
                return;

            List<TileBase> remainingTiles = new List<TileBase>();

            for (int i = newPosition + 1; i < playerTileGrid.GridSize.x + 1; i++)
            {
                remainingTiles.AddRange(GetUnitTilesInPositionAndSize(i).Keys);
                remainingTiles.AddRange(GetMonsterTilesInPositionAndSize(i).Keys);
            }

            if (remainingTiles.Count == 0)
            {
                noRemainingTilesToPlay?.Invoke();
            }
        }

        public Dictionary<UnitTile, int> GetUnitTilesInPositionAndSize(int position)
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
                    {
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
            }

            return returnTiles;
        }

        public Dictionary<MonsterTile, int> GetMonsterTilesInPositionAndSize(int position)
        {
            Dictionary<MonsterTile, int> returnTiles = new Dictionary<MonsterTile, int>();

            if (position >= playerTileGrid.Tiles.GetLength(0))
            {
                return returnTiles;
            }

            for (int i = 0; i < playerTileGrid.Tiles.GetLength(1); i++)
            {
                if (playerTileGrid.Tiles[position, i] != null)
                {
                    if (playerTileGrid.Tiles[position, i] is MonsterTile monsterTile)
                    {
                        if (returnTiles.ContainsKey(monsterTile))
                        {
                            returnTiles[monsterTile] += 1;
                        }
                        else
                        {
                            returnTiles.Add(monsterTile, 1);
                        }
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


