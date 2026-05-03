using UnityEngine;
using Tiles;

public class TileRotationHelper : MonoBehaviour
{
    [SerializeField]
    private RectTransform tileSizeHolder;

    public void Follow(TileUI tile)
    {
        transform.position = tile.RectTransform.position;

        bool tileRotated = Mathf.RoundToInt(tile.Rotation / 90f) % 2 == 1;

        float x = tileRotated ? tile.RectTransform.sizeDelta.y : tile.RectTransform.sizeDelta.x;
        float y = tileRotated ? tile.RectTransform.sizeDelta.x : tile.RectTransform.sizeDelta.y;
        tileSizeHolder.sizeDelta = new Vector2(x, y);
    }
}
