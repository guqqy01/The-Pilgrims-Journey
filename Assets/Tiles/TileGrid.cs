using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class TileGrid : MonoBehaviour
{
    [SerializeField]
    private RectTransform gridTransform;
    public RectTransform GridTransform => gridTransform;

    protected Tile[,] tiles;

    public Tile this[int i, int j]
    {
        get
        {
            return tiles[i, j];
        }
        set
        {
            tiles[i, j] = value;
        }
    }

    public Tile this[Vector2Int position]
    {
        get
        {
            return tiles[position.x - 1, position.y - 1];
        }
        set
        {
            tiles[position.x - 1, position.y - 1] = value;
        }
    }

    public List<Tile> Tiles 
    {
        get
        {
            List<Tile> returnList = new List<Tile>();

            foreach (var tile in tiles)
            {
                if (tile != null)
                    returnList.Add(tile);
            }

            return returnList;
        }
    }

    public delegate void OnTileChange();
    public OnTileChange onTileChange;

    [SerializeField]
    protected Vector2Int tileGridSize;
    public Vector2Int TileGridSize => tileGridSize;

    protected virtual void Awake()
    {
        tiles = new Tile[tileGridSize.x, tileGridSize.y];
    }

    public bool PointOutOfGrid(Vector2Int point) => point.x > tileGridSize.x || point.x <= 0 || point.y > tileGridSize.y || point.y <= 0;

    public bool CanPlaceTile(Tile tile, Vector2Int position)
    {
        foreach(Vector2Int point in tile.Shape)
        {
            if(PointOutOfGrid(point + position) || 
                (TryGetTileAtPosition(point + position, out Tile outTile) && outTile != tile))
            {
                return false;
            }
        }

        return true;
    }

    public bool HoverTileCanPlace(Tile hover, Tile selected, Vector2Int position)
    {
        foreach (Vector2Int point in hover.Shape)
        {
            if (PointOutOfGrid(point + position) ||
                (TryGetTileAtPosition(point + position, out Tile outTile) && outTile != selected))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryPlaceTile(Tile selectedTile, Vector2Int position)
    {
        if (!CanPlaceTile(selectedTile, position))
            return false;

        RemoveTile(selectedTile);

        foreach(var point in selectedTile.Shape.AddToAll(position))
        {
            tiles[point.x - 1, point.y - 1] = selectedTile;
        }

        onTileChange?.Invoke();
        return true;
    }


    public bool TryGetTileAtPosition(Vector2Int v2Int, out Tile tile)
    {
        tile = null;

        if (PointOutOfGrid(v2Int))
            return false;

        tile = this[v2Int];

        return tile != null;
    }

    public bool RemoveTile(Tile tile)
    {
        bool haveRemovedTile = false;

        for (int i = 0; i < tileGridSize.x; i++)
        {
            for (int j = 0; j < tileGridSize.y; j++)
            {
                if (tiles[i, j] == tile)
                {
                    tiles[i, j] = null;
                    haveRemovedTile = true;
                }
            }
        }
        return haveRemovedTile;
    }
}
