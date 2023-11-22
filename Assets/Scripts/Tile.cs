using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public List<Vector2Int> shape;

    public GameObject unit;
}

public class TilePlayer : MonoBehaviour
{
    public Vector2Int tilePlayerSize;

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

            return true;
        }

        return false;
    }

    public bool RemoveTile(Tile tile)
    {
        if (tileAndOccupiedSpot.ContainsKey(tile))
        {
            tileAndOccupiedSpot.Remove(tile);
            return true;
        }

        return false;
    }
}
