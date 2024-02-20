using System.Collections.Generic;
using UnityEngine;
using Map;
using ModularMotion;
using Units;
public class TilePlayer : TileGrid
{
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

    private UnitSpawningInfo previousSpawningUnits;
    private UnitSpawningInfo currentSpawningUnits;

    private UIMotion uIMotion;

    protected override void Awake()
    {
        base.Awake();
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

        int posBefore = Mathf.FloorToInt(CurrentPlayingPosition) + 1;

        CurrentPlayingPosition += Time.deltaTime / oneTileTime;
        playerTime += Time.deltaTime;

        playerUIPoint.position = GridTransform.position + new Vector3(-GridTransform.sizeDelta.x + 2f * CurrentPlayingPosition * (GridTransform.sizeDelta.x / TileGridSize.x), GridTransform.sizeDelta.y + playerUIPointYOffset) * canvas.scaleFactor / 2f;

        if (posBefore != Mathf.FloorToInt(CurrentPlayingPosition) + 1)
        {
            PositionChanged();
            onCurrentPlayingPositionChange?.Invoke();
        }

        if (PathPicker.instance.pickedDirection != null)
        {
            Direction pathDirection = (Direction)PathPicker.instance.pickedDirection;

            for (int i = 0; i < currentSpawningUnits.Count; i++)
            {
                if (playerTime >= currentSpawningUnits[i].nextSpawnTime)
                {
                    Unit unit = Instantiate(currentSpawningUnits[i].tile.unit.unitPrefab, (Vector2)LocalMapGenerator.Instance.pathway.PathNodes[pathDirection][0], Quaternion.identity);
                    unit.Init(currentSpawningUnits[i].tile.unit, currentSpawningUnits[i].tileStats ,LocalMapGenerator.Instance.pathway.PathNodes[pathDirection]);
                    currentSpawningUnits[i].nextSpawnTime = playerTime + 1f / currentSpawningUnits[i].spawnRate;
                    
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

        Dictionary<Tile, int> tilesInPosition = GetTilesInPositionAndSize(Mathf.FloorToInt(CurrentPlayingPosition) + 1);
        foreach (KeyValuePair<Tile, int> tile in tilesInPosition)
        {
            tile.Key.locked = true;
        }
    }

    public void Stop()
    {
        foreach(Tile tile in Tiles)
        {
            tile.locked = false;
        }
        isPlaying = false;
        playerTime = 0;
    }

    public void PositionChanged()
    {
        Dictionary<Tile, int> tilesInPosition = GetTilesInPositionAndSize(Mathf.FloorToInt(CurrentPlayingPosition) + 1);

        previousSpawningUnits = currentSpawningUnits;

        currentSpawningUnits = new UnitSpawningInfo(tilesInPosition, previousSpawningUnits, Mathf.FloorToInt(CurrentPlayingPosition) + 1);

        if (!isPlaying)
            return;

        foreach (KeyValuePair<Tile, int> tile in tilesInPosition)
        {
            tile.Key.locked = true;
        }
    }

    public void TileChanged()
    {

    }

    public Dictionary<Tile, int> GetTilesInPositionAndSize(int position)
    {
        Dictionary<Tile, int> returnTiles = new Dictionary<Tile, int>();

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
