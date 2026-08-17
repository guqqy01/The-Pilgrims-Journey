using Codice.CM.Client.Differences;
using UnityEngine;

namespace Tiles.UI
{
    public class TileRotationHelper : MonoBehaviour
    {
        [SerializeField]
        private RectTransform tileSizeHolder;

        public void Follow(TileUIBase tile)
        {
            if (tile == null)
                return;
            transform.position = tile.RectTransform.position;

            bool tileRotated = Mathf.RoundToInt(tile.UIRotation / 90f) % 2 == 1;

            if (tile.TileGraphics == null) return;

            float x = tileRotated ? tile.TileGraphics.rectTransform.rect.size.y : tile.TileGraphics.rectTransform.rect.size.x;
            float y = tileRotated ? tile.TileGraphics.rectTransform.rect.size.x : tile.TileGraphics.rectTransform.rect.size.y;
            tileSizeHolder.sizeDelta = new Vector2(x, y);
        }
    }

}

