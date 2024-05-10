using System.Collections.Generic;
using UnityEngine;
using Map;
using ModularMotion;
using Units;
public class TilePlayer : TileGrid
{
    public static TilePlayer Instance;

    private float playerTime = 0;
    public float CurrentPlayingPosition { private set; get; }

    public delegate void OnCurrentPlayingPositionChange();
    public OnCurrentPlayingPositionChange onCurrentPlayingPositionChange;

    [SerializeField]
    private float oneTileTime = 15f;

    [SerializeField]
    private bool isPlaying = false;

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private RectTransform playerUIPoint;
    [SerializeField]
    private float playerUIPointYOffset;

    private Dictionary<Tile, int> spawningTilesAndSize = new Dictionary<Tile, int>();

    private UIMotion uIMotion;

    private List<Unit> spawnedUnits = new List<Unit>();

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        uIMotion = GetComponent<UIMotion>();
    }

    private void Start()
    {
        PositionChanged();
        playerUIPoint.position = GridTransform.position + new Vector3(-GridTransform.sizeDelta.x, GridTransform.sizeDelta.y + playerUIPointYOffset) * canvas.scaleFactor / 2f;
        onTileChange += PositionChanged;
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        int posBefore = Mathf.FloorToInt(CurrentPlayingPosition);

        CurrentPlayingPosition += Time.deltaTime / oneTileTime;
        playerTime += Time.deltaTime;

        playerUIPoint.position = GridTransform.position + new Vector3(-GridTransform.sizeDelta.x + 2f * CurrentPlayingPosition * (GridTransform.sizeDelta.x / TileGridSize.x), GridTransform.sizeDelta.y + playerUIPointYOffset) * canvas.scaleFactor / 2f;

        if (posBefore != Mathf.FloorToInt(CurrentPlayingPosition))
        {
            PositionChanged();
            onCurrentPlayingPositionChange?.Invoke();
        }

        if (PathPicker.Instance.PickedDirection != null)
        {
            Direction pathDirection = (Direction)PathPicker.Instance.PickedDirection;

            foreach (KeyValuePair<Tile, int> tileAndSize in spawningTilesAndSize)
            {
                Tile tile = tileAndSize.Key;
                int size = tileAndSize.Value;

                if (playerTime >= tile.TimeForNextSpawn)
                {
                    Unit unit = Instantiate(tile.Unit.unitPrefab, (Vector2)LocalMapGenerator.Instance.pathway.PathNodes[pathDirection][0], Quaternion.identity);
                    unit.Init(tile, LocalMapGenerator.Instance.pathway.PathNodes[pathDirection], 0);
                    spawnedUnits.Add(unit);
                    tile.TimeForNextSpawn = playerTime + 1f / (tile.SpawnRateValue * size);

                }
            } 
        }
    }

    public void OpenClosePlayer()
    {
        uIMotion.Play();
    }

    public void PlayAndPause()
    {
        isPlaying = !isPlaying;

        if (!isPlaying)
            return;

        Dictionary<Tile, int> tilesInPosition = GetTilesInPositionAndSize(Mathf.FloorToInt(CurrentPlayingPosition));
        foreach (KeyValuePair<Tile, int> tile in tilesInPosition)
        {
            tile.Key.locked = true;
        }
    }

    public void StopAndReset()
    {
        foreach(Tile tile in Tiles)
        {
            tile.locked = false;
            tile.TimeForNextSpawn = 0;
        }

        foreach(Unit unit in spawnedUnits)
        {
            if(unit != null)
            {
                Destroy(unit.gameObject);
            }
        }

        spawnedUnits.Clear();

        isPlaying = false;
        CurrentPlayingPosition = 0;
        playerTime = 0;

        PositionChanged();
        playerUIPoint.position = GridTransform.position + new Vector3(-GridTransform.sizeDelta.x, GridTransform.sizeDelta.y + playerUIPointYOffset) * canvas.scaleFactor / 2f;
    }

    public void PositionChanged()
    {
        spawningTilesAndSize = GetTilesInPositionAndSize(Mathf.FloorToInt(CurrentPlayingPosition));

        if (!isPlaying)
            return;

        foreach (KeyValuePair<Tile, int> tile in spawningTilesAndSize)
        {
            tile.Key.locked = true;
        }
    }

    public Dictionary<Tile, int> GetTilesInPositionAndSize(int position)
    {
        Dictionary<Tile, int> returnTiles = new Dictionary<Tile, int>();

        if (position >= tiles.GetLength(0))
            return returnTiles;

        for (int i = 0; i < tiles.GetLength(1); i++)
        {
            if (tiles[position, i] != null)
            {
                if (returnTiles.ContainsKey(tiles[position, i]))
                {
                    returnTiles[tiles[position, i]] += 1;
                }
                else
                {
                    returnTiles.Add(tiles[position, i], 1);
                }
            }
        }

        return returnTiles;
    }
}
