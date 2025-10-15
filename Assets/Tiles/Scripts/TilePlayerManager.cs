using GameManagerCalls;
using Map;
using System.Collections.Generic;
using Units;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tiles
{
    public class TilePlayerManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot, IOnCompletedLevel, IOnSetup
    {
        public static TilePlayerManager Instance { private set; get; }
        public float CurrentPlayingPosition { private set; get; }

        public delegate void OnCurrentPlayingPositionChange(Dictionary<Tile, int> spawningTilesAndSize);
        public OnCurrentPlayingPositionChange onPositionChangeNextSpawning;
        public OnCurrentPlayingPositionChange onPositionChangeCurrentSpawning;

        public delegate void NoRemainingTilesToPlay();
        public NoRemainingTilesToPlay noRemainingTilesToPlay;

        public delegate void OnPlay();
        public OnPlay onPlay;

        [SerializeField]
        private float oneTileTime = 15f;

        public bool IsPlaying { private set; get; } = false;

        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private RectTransform playerUIPoint;
        [SerializeField]
        private float playerUIPointYOffset;

        public Dictionary<Tile, int> SpawningTilesAndSize { private set; get; } = new Dictionary<Tile, int>();
        public Dictionary<Tile, int> NextSpawningTilesAndSize { private set; get; } = new Dictionary<Tile, int>();

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
        private TileGrid playerTileGrid;

        public void InitiateSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public void OnEnable()
        {
            playerUIPoint.position = playerTileGrid.GridTransform.position + new Vector3(-playerTileGrid.GridTransform.sizeDelta.x, playerTileGrid.GridTransform.sizeDelta.y + playerUIPointYOffset) * canvas.scaleFactor / 2f;
        }

        private void Update()
        {
            if (!IsPlaying)
                return;

            int posBefore = Mathf.FloorToInt(CurrentPlayingPosition);

            CurrentPlayingPosition += Time.deltaTime / oneTileTime;

            playerUIPoint.position = playerTileGrid.GridTransform.position + new Vector3(-playerTileGrid.GridTransform.sizeDelta.x + 2f * CurrentPlayingPosition * (playerTileGrid.GridTransform.sizeDelta.x / playerTileGrid.GridSize.x), playerTileGrid.GridTransform.sizeDelta.y + playerUIPointYOffset) * canvas.scaleFactor / 2f;

            if (posBefore != Mathf.FloorToInt(CurrentPlayingPosition))
            {
                PositionChanged();
            }
        }

        public void Setup()
        {
            StopPlaying();

            if (playerTileGrid.PointOutOfGrid(startTilePosition))
            {
                Debug.LogWarning($"Starting tile position is out of the grid");
                return;
            }

            Tile tile = Instantiate(startingTile.tilePrefab, playerTileGrid.GridTransform);
            tile.BaseInit(startingTile, startingUnit);
            tile.RotateTo(startTileRotation);

            if(!playerTileGrid.TryPlaceTile(tile, startTilePosition))
            {
                Debug.LogWarning($"Could not place tile make sure placement and size of tile fits on the tile grid, Placing anywhere thats possible");
                playerTileGrid.PlaceTiles(new List<TileAndUnit>() { new TileAndUnit(startingTile, startingUnit)});
                Destroy(tile.gameObject);
            }
        }


        public void Play()
        {
            if (PathPicker.Instance.PickedDirection == null)
                return;

            IsPlaying = true;

            PositionChanged();

            foreach (Tile tile in playerTileGrid.ListOfTiles)
            {
                tile.locked = true;
            }

            onPlay?.Invoke();
        }

        public void StopPlaying()
        {
            foreach (Tile tile in playerTileGrid.ListOfTiles)
            {
                tile.locked = false;
            }

            IsPlaying = false;
            CurrentPlayingPosition = 0;

            playerUIPoint.position = playerTileGrid.GridTransform.position + new Vector3(-playerTileGrid.GridTransform.sizeDelta.x, playerTileGrid.GridTransform.sizeDelta.y + playerUIPointYOffset) * canvas.scaleFactor / 2f;
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

            List<Tile> remainingTiles = new List<Tile>();

            for (int i = newPosition + 1; i < playerTileGrid.GridSize.x + 1; i++)
            {
                remainingTiles.AddRange(GetTilesInPositionAndSize(i).Keys);
            }

            if (remainingTiles.Count == 0)
            {
                noRemainingTilesToPlay?.Invoke();
            }
        }

        public Dictionary<Tile, int> GetTilesInPositionAndSize(int position)
        {
            Dictionary<Tile, int> returnTiles = new Dictionary<Tile, int>();

            if (position >= playerTileGrid.Tiles.GetLength(0))
            {
                return returnTiles;
            }

            for (int i = 0; i < playerTileGrid.Tiles.GetLength(1); i++)
            {
                if (playerTileGrid.Tiles[position, i] != null)
                {
                    if (returnTiles.ContainsKey(playerTileGrid.Tiles[position, i]))
                    {
                        returnTiles[playerTileGrid.Tiles[position, i]] += 1;
                    }
                    else
                    {
                        returnTiles.Add(playerTileGrid.Tiles[position, i], 1);
                    }
                }
            }

            return returnTiles;
        }

        public void OnLevelCompleted()
        {
            StopPlaying();
        }
    }
}


