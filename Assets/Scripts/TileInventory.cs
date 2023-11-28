using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private Vector2Int inventorySize;

    private bool PointOutOfPlayer(Vector2Int point) => point.x > inventorySize.x || point.x <= 0 || point.y > inventorySize.y || point.y <= 0;

    public bool CanPlaceTile(Tile tile, Vector2Int position)
    {
        foreach(Vector2Int point in tile.Shape)
        {
            if(PointOutOfPlayer(point + position) || occupiedSpots.Contains(point + position))
            {
                return false;
            }
        }

        return true;
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

    }
}
