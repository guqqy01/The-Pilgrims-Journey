using System.Collections.Generic;
using UnityEngine;
using Map;

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

    private UnitSpawningInfo previousSpawningUnits;
    private UnitSpawningInfo currentSpawningUnits;

    private void Start()
    {
        PositionChanged();
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        int posBefore = Mathf.FloorToInt(CurrentPlayingPosition) + 1;

        CurrentPlayingPosition += Time.deltaTime / oneTileTime;
        playerTime += Time.deltaTime;

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
                    Movement mov = Instantiate(currentSpawningUnits[i].unit.movement, (Vector2)LocalMapGenerator.Instance.pathway.PathNodes[pathDirection][0], Quaternion.identity);
                    mov.Init(LocalMapGenerator.Instance.pathway.PathNodes[pathDirection]);
                    currentSpawningUnits[i].nextSpawnTime = playerTime + 1f / currentSpawningUnits[i].spawnRate;
                    
                }
            }
        }
    }

    public void PlayAndPause()
    {
        isPlaying = !isPlaying;
    }

    public void Stop()
    {
        isPlaying = false;
        playerTime = 0;
    }

    public void PositionChanged()
    {
        List<Tile> tiles = GetTilesInPosition(Mathf.FloorToInt(CurrentPlayingPosition) + 1);

        previousSpawningUnits = currentSpawningUnits;

        currentSpawningUnits = new UnitSpawningInfo(tiles, previousSpawningUnits, tileAndOccupiedSpot, Mathf.FloorToInt(CurrentPlayingPosition) + 1);

        foreach (Tile tile in tiles)
        {
            tile.locked = true;
        }
    }

    public List<Tile> GetTilesInPosition(int position)
    {
        List<Tile> tiles = new List<Tile>();
        bool IsTileInPosition(KeyValuePair<Tile, List<Vector2Int>> kvpTile)
        {
            for (int i = 0; i < kvpTile.Value.Count; i++)
            {
                Vector2Int tileSpot = kvpTile.Value[i];
                if (position == tileSpot.x)
                    return true;
            }
            return false;
        }

        foreach (var kvp in tileAndOccupiedSpot)
        {
            if (IsTileInPosition(kvp))
                tiles.Add(kvp.Key);
        }

        return tiles;
    }
}
