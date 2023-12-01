using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class TileGrid : MonoBehaviour
{
    [SerializeField]
    private RectTransform gridTransform;
    public RectTransform GridTransform => gridTransform;

    protected Dictionary<Tile, List<Vector2Int>> tileAndOccupiedSpot = new Dictionary<Tile, List<Vector2Int>>();

    public List<Vector2Int> OccupiedSpots
    {
        get
        {
            List<Vector2Int> spots = new List<Vector2Int>();

            foreach(KeyValuePair<Tile, List<Vector2Int>> kvp in tileAndOccupiedSpot)
            {
                spots.AddRange(kvp.Value);
            }

            return spots;
        }
    }

    public List<Tile> Tiles => tileAndOccupiedSpot.Keys.ToList();

    public delegate void OnTileChange();
    public OnTileChange onTileChange;

    [SerializeField]
    protected Vector2Int tileGridSize;
    public Vector2Int TileGridSize => tileGridSize;

    public bool PointOutOfGrid(Vector2Int point) => point.x > tileGridSize.x || point.x <= 0 || point.y > tileGridSize.y || point.y <= 0;

    public bool CanPlaceTile(Tile tile, Vector2Int position)
    {
        foreach(Vector2Int point in tile.Shape)
        {
            if(PointOutOfGrid(point + position) || (TryGetTileAtPosition(point, out Tile outTile) && outTile != tile))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryPlaceTile(Tile tile, Vector2Int position)
    {
        if (!CanPlaceTile(tile, position))
            return false;

        if (tileAndOccupiedSpot.ContainsKey(tile))
        {
            tileAndOccupiedSpot[tile] = tile.Shape.AddToAll(position);
        }
        else
        {
            tileAndOccupiedSpot.Add(tile, tile.Shape.AddToAll(position));
        }

        onTileChange?.Invoke();
        return true;
    }


    public bool TryGetTileAtPosition(Vector2Int v2Int, out Tile tile)
    {
        tile = null;

        if (PointOutOfGrid(v2Int))
            return false;

        foreach (var kvp in tileAndOccupiedSpot)
        {
            if (kvp.Value.Contains(v2Int))
            {
                tile = kvp.Key;
                return true;
            }
        }

        return false;
    }

    public bool RemoveTile(Tile tile)
    {
        return tileAndOccupiedSpot.Remove(tile);
    }
}
