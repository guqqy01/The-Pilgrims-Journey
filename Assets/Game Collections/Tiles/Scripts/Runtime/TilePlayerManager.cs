using GameManagerCalls;
using LocalMap;
using Spells;
using System;
using System.Collections.Generic;
using TPJ.Gameplay;
using UnityEngine;

namespace Tiles
{
    public class TilePlayerManager : Singleton<TilePlayerManager>, IOnCompleteLocalMapLevel, IOnSetupNewGame, IOnClear
    {
        public float CurrentPlayingPosition { private set; get; }

        public delegate void OnPositionChangeUnitTiles(Dictionary<UnitTile, int> spawningTilesAndSize);
        public event OnPositionChangeUnitTiles onPositionChangeNextUnitTiles;
        public event OnPositionChangeUnitTiles onPositionChangeCurrentUnitTiles;

        public delegate void OnPositionChangeMonsterTiles(Dictionary<MonsterTileBase, int> spawningTilesAndSize);
        public event OnPositionChangeMonsterTiles onPositionChangeNextMonsterTiles;
        public event OnPositionChangeMonsterTiles onPositionChangeCurrentMonsterTiles;

        public delegate void NoRemainingTilesToPlay();
        public NoRemainingTilesToPlay noRemainingTilesToPlay;

        public delegate void OnPlay();
        public OnPlay onPlay;

        [SerializeField]
        private float oneTileTime = 15f; // seconds per timeline slot

        public bool IsPlaying { private set; get; } = false;

        public SpawnTimeline Timeline { get; } = new SpawnTimeline();

        public Dictionary<UnitTile, int> SpawningUnitTilesAndSize { private set; get; } = new Dictionary<UnitTile, int>();
        public Dictionary<UnitTile, int> NextSpawningUnitTilesAndSize { private set; get; } = new Dictionary<UnitTile, int>();

        public Dictionary<MonsterTileBase, int> SpawningMonsterTilesAndSize { private set; get; } = new Dictionary<MonsterTileBase, int>();
        public Dictionary<MonsterTileBase, int> NextSpawningMonsterTilesAndSize { private set; get; } = new Dictionary<MonsterTileBase, int>();

        public Action OnStopPlaying;

        protected override void Awake()
        {
            base.Awake();
            Timeline.TileAdded += OnTileAdded;
            Timeline.TileRemoved += OnTileRemoved;
        }

        protected override void CleanUp()
        {
            onPositionChangeNextUnitTiles = null;
            onPositionChangeCurrentUnitTiles = null;
            noRemainingTilesToPlay = null;
            onPlay = null;
            OnStopPlaying = null;
            onPositionChangeNextMonsterTiles = null;
            onPositionChangeCurrentMonsterTiles = null;

            Timeline.TileAdded -= OnTileAdded;
            Timeline.TileRemoved -= OnTileRemoved;
        }

        private void OnTileAdded(TileBase tile)
        {
            if (tile is not UnitTile unitTile || unitTile.UnitSO?.unitsSpecialSpell == null)
                return;

            SpellManager.Instance.AddSpell(unitTile.UnitSO.unitsSpecialSpell, tile.Shape.Count);
        }

        private void OnTileRemoved(TileBase tile)
        {
            if (tile is not UnitTile unitTile || unitTile.UnitSO?.unitsSpecialSpell == null)
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
                PositionChanged();
        }

        public void OnSetupNewGame()
        {
            StopPlaying();
            Timeline.Clear();
        }

        public void Play()
        {
            if (PathPicker.Instance.PickedDirection == null)
                return;

            IsPlaying = true;

            PositionChanged();

            foreach (TileBase tile in Timeline.Tiles)
                tile.Locked = true;

            onPlay?.Invoke();
        }

        public void StopPlaying()
        {
            foreach (TileBase tile in Timeline.Tiles)
            {
                if (tile is UnitTile unitTile)
                    unitTile.Locked = false;
            }

            IsPlaying = false;
            CurrentPlayingPosition = 0;

            OnStopPlaying?.Invoke();
        }

        public void PositionChanged()
        {
            int newPosition = Mathf.FloorToInt(CurrentPlayingPosition);

            SpawningUnitTilesAndSize = Timeline.GetUnitTilesAt(newPosition);
            NextSpawningUnitTilesAndSize = Timeline.GetUnitTilesAt(newPosition + 1);

            SpawningMonsterTilesAndSize = Timeline.GetMonsterTilesAt(newPosition);
            NextSpawningMonsterTilesAndSize = Timeline.GetMonsterTilesAt(newPosition + 1);

            onPositionChangeCurrentUnitTiles?.Invoke(SpawningUnitTilesAndSize);
            onPositionChangeNextUnitTiles?.Invoke(NextSpawningUnitTilesAndSize);

            onPositionChangeCurrentMonsterTiles?.Invoke(SpawningMonsterTilesAndSize);
            onPositionChangeNextMonsterTiles?.Invoke(NextSpawningMonsterTilesAndSize);

            if (!IsPlaying || SpawningUnitTilesAndSize.Count > 0)
                return;

            if (!Timeline.HasTilesAfter(newPosition))
                noRemainingTilesToPlay?.Invoke();
        }

        // Kept so existing callers keep compiling.
        public Dictionary<UnitTile, int> GetUnitTilesInPositionAndSize(int position) => Timeline.GetUnitTilesAt(position);
        public Dictionary<MonsterTileBase, int> GetMonsterTilesInPositionAndSize(int position) => Timeline.GetMonsterTilesAt(position);

        public void OnCompleteLocalMapLevel() => StopPlaying();

        public void Clear()
        {
            StopPlaying();
            Timeline.Clear();
        }
    }
}