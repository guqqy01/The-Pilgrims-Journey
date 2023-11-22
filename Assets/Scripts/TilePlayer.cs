using System.Collections.Generic;
using UnityEngine;

public class TilePlayer : MonoBehaviour
{
    public Vector2Int tilePlayerSize;

    private float playerTime = 0;
    public float CurrentPosition { private set; get; }

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

    private Dictionary<Unit, float> unitsAndSpawnRate = new Dictionary<Unit, float>();
    private Dictionary<Unit, float> unitsAndNextSpawnTime = new Dictionary<Unit, float>();

    private void Update()
    {
        if (!isPlaying)
            return;

        CurrentPosition += Time.deltaTime / oneTileTime;
        playerTime += Time.deltaTime;

        
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Stop()
    {
        isPlaying = false;
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
        if (tileAndOccupiedSpot.ContainsKey(tile))
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
