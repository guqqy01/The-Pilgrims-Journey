using LocalMap;
using System.Collections.Generic;
using UnityEngine;

public class MapTileHighlightManager : Singleton<MapTileHighlightManager>
{
    private List<MapTile> _highlightedTiles = new List<MapTile>();

    public class HighlightInfo
    {
        public List<Vector2Int> Shape;
        public Color Color;

        public HighlightInfo(Color color,
            List<Vector2Int> shape)
        {
            Color = color;
            Shape = shape;
        }
    }

    public void HideHighlight()
    {
        if (_highlightedTiles == null || _highlightedTiles.Count == 0)
            return;

        for (int i = 0; i < _highlightedTiles.Count; i++)
        {
            if (_highlightedTiles[i] != null)
                _highlightedTiles[i].Highlighted = false;
        }

        _highlightedTiles.Clear();
    }

    public void ShowHighlight(List<HighlightInfo> highlightInfos)
    {
        foreach (HighlightInfo highlightInfo in highlightInfos)
        {
            List<Vector2Int> v2IntList = highlightInfo.Shape;

            foreach (Vector2Int v2Int in v2IntList)
            {
                if (LocalMapGenerator.Instance.Tiles != null && 
                    v2Int.x >= 0 && 
                    v2Int.x < LocalMapGenerator.Instance.Tiles.GetLength(0) && 
                    v2Int.y >= 0 && 
                    v2Int.y < LocalMapGenerator.Instance.Tiles.GetLength(1))
                {
                    bool[,] boolArray = new bool[3, 3];

                    for (int i = 0; i < boolArray.GetLength(0); i++)
                    {
                        for (int j = 0; j < boolArray.GetLength(1); j++)
                        {
                            boolArray[i, j] = !v2IntList.Contains(v2Int + new Vector2Int(i, j) - new Vector2Int(1, 1));
                        }
                    }
                    Color? color = highlightInfo.Color;

                    if (!color.HasValue)
                        return;

                    MapTile tile = LocalMapGenerator.Instance.Tiles[v2Int.x, v2Int.y];
                    tile.HighlightWithColor(color.Value, boolArray);
                    _highlightedTiles.Add(LocalMapGenerator.Instance.Tiles[v2Int.x, v2Int.y]);
                }

            }
        }
    }
}
