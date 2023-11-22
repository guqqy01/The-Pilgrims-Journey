using System.Collections.Generic;
using UnityEngine;
using Map;

public class TilePlayer : MonoBehaviour
{
    public Vector2Int tilePlayerSize;

    private float playerTime = 0;
    public float CurrentPosition { private set; get; }

    public delegate void OnPositionChange();
    public OnPositionChange onPositionChange;

    [SerializeField]
    private float oneTileTime = 15f;

    public List<Vector2Int> OccupiedSpaces
    {
        get
        {
            List<Vector2Int> list = new List<Vector2Int>();

            foreach(var kvp in tileAndOccupiedSpot)
            {
                list.AddRange(kvp.Value);
            }

            return list;
        }
    }

    Dictionary<Tile, List<Vector2Int>> tileAndOccupiedSpot = new Dictionary<Tile, List<Vector2Int>>();

    public delegate void OnTileChange();
    public OnTileChange onTileChange;

    private bool isPlaying = false;

    private UnitSpawningInfo previousSpawningUnits;
    private UnitSpawningInfo currentSpawningUnits;



    private void Awake()
    {
        
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        int posBefore = Mathf.CeilToInt(CurrentPosition);

        CurrentPosition += Time.deltaTime / oneTileTime;
        playerTime += Time.deltaTime;

        if (posBefore != Mathf.CeilToInt(CurrentPosition))
        {
            PositionChanged();
            onPositionChange?.Invoke();
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
        List<Tile> tiles = GetTilesAtCurrentPosition();

        previousSpawningUnits = currentSpawningUnits;

        currentSpawningUnits = new UnitSpawningInfo(tiles, previousSpawningUnits, tileAndOccupiedSpot, Mathf.CeilToInt(CurrentPosition));

        foreach (Tile tile in tiles)
        {
            tile.locked = true;
        }
    }

    private bool PointOutOfPlayer(Vector2Int point) => point.x > tilePlayerSize.x || point.x <= 0 || point.y > tilePlayerSize.y || point.y <= 0;

    public bool CanPlaceTile(List<Vector2Int> tileShape, Vector2Int tilePosition)
    {
        for (int i = 0; i < tileShape.Count; i++)
        {
            Vector2Int point = tileShape[i] + tilePosition;

            if (OccupiedSpaces.Contains(point) || PointOutOfPlayer(point))
                return false;
        }

        return true;
    }

    public bool TryPlaceTile(Tile tile, Vector2Int position)
    {
        if (CanPlaceTile(tile.shape, position))
        {
            List<Vector2Int> occupiedSpot = new List<Vector2Int>();

            for (int i = 0; i < tile.shape.Count; i++)
            {
                occupiedSpot.Add(tile.shape[i] + position);
            }

            tileAndOccupiedSpot.Add(tile, occupiedSpot);
            onTileChange?.Invoke();
            return true;
        }

        return false;
    }

    public bool RemoveTile(Tile tile)
    {
        if (tileAndOccupiedSpot.ContainsKey(tile) && !tile.locked)
        {
            tileAndOccupiedSpot.Remove(tile);
            onTileChange?.Invoke();
            return true;
        }

        return false;
    }

    public List<Tile> GetTilesAtCurrentPosition()
    {
        return GetTilesInPosition(Mathf.CeilToInt(CurrentPosition));
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

public class UnitSpawningInfo
{
    public List<UnitSpawnInfo> unitSpawnInfo = new List<UnitSpawnInfo>();

    public int Count => unitSpawnInfo.Count;

    public UnitSpawnInfo this[int i]
    {
        get
        {
            return unitSpawnInfo[i];
        }
    }

    public UnitSpawnInfo this[Unit unit]
    {
        get
        {
            for (int i = 0; i < unitSpawnInfo.Count; i++)
            {
                if (unitSpawnInfo[i].unit == unit)
                {
                    return unitSpawnInfo[i];
                }
            }

            return null;
        }
    }

    public UnitSpawningInfo(List<Tile> tiles, UnitSpawningInfo previous, Dictionary<Tile, List<Vector2Int>> tileAndSpaces, int position)
    {
        unitSpawnInfo = new List<UnitSpawnInfo>();

        foreach (Tile tile in tiles)
        {
            int multiplier = 0;
            foreach(Vector2Int v2int in tileAndSpaces[tile])
            {
                if (v2int.x - position == 0)
                    multiplier++;
            }

            unitSpawnInfo.Add(new UnitSpawnInfo(tile, previous.ContainsUnit(tile.unit) ? previous[tile.unit].nextSpawnTime : Time.time, multiplier));
        }
    }

    public bool ContainsUnit(Unit unit)
    {
        foreach(UnitSpawnInfo info in unitSpawnInfo)
        {
            if (info.unit == unit)
                return true;
        }

        return false;
    }
}

public class UnitSpawnInfo
{
    public Unit unit;
    public float spawnRate;
    public float nextSpawnTime;

    public UnitSpawnInfo(Tile tile, float nextSpawnTime, int spawnRateMultiplier)
    {
        unit = tile.unit;
        spawnRate = unit.baseSpawnRate * spawnRateMultiplier;
        this.nextSpawnTime = nextSpawnTime;
    }
}
