using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TilePlayer;

public class TileInventory : MonoBehaviour
{
    Dictionary<Tile, List<Vector2Int>> tileAndOccupiedSpot = new Dictionary<Tile, List<Vector2Int>>();

    public List<Vector2Int> occupiedSpots
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

    public delegate void OnChange();
    public OnChange onChange;

    [SerializeField]
    private Vector2Int tileInventorySize;
    public Vector2Int TileInventorySize => tileInventorySize;

    private bool PointOutOfPlayer(Vector2Int point) => point.x > tileInventorySize.x || point.x <= 0 || point.y > tileInventorySize.y || point.y <= 0;

    public bool CanPlaceTile(Tile tile, Vector2Int position)
    {
        foreach(Vector2Int point in tile.Shape)
        {
            if(PointOutOfPlayer(point + position) || (TryGetTileAtPosition(point, out Tile outTile) && outTile != tile))
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

        tileAndOccupiedSpot.Add(tile, tile.Shape.AddToAll(position));
        return true;
    }


    public bool TryGetTileAtPosition(Vector2Int v2Int, out Tile tile)
    {
        tile = null;

        if (PointOutOfPlayer(v2Int))
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

    public bool AddTile(Tile tile, Vector2Int position)
    {
        if(CanPlaceTile(tile, position))
        {
            tileAndOccupiedSpot.Add(tile, tile.Shape.AddToAll(position));

            return true;
        }

        return false;
    }

    public bool RemoveTile(Tile tile)
    {
        return tileAndOccupiedSpot.Remove(tile);
    }
}
