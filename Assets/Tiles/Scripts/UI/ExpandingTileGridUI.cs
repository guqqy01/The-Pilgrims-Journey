using UnityEngine;
using UnityEngine.UI;

namespace Tiles
{
    public class ExpandingTileGridUI : TileGridUI
    {
        protected ExpandingTileGridBase _expandingTileGrid;

        [SerializeField]
        protected Image gridImage;

        [SerializeField]
        private RectTransform contentRect;

        private void Awake()
        {
            if (tileGrid != null && tileGrid is ExpandingTileGridBase expandingTileGrid)
            {
                _expandingTileGrid = expandingTileGrid;
                _expandingTileGrid.onGridResized += OnGridResized;

                OnGridResized();
            }
            else
            {
                Debug.Log($"Tile grid null or not an expanding tile grid");
            }
        }

        private void OnDestroy()
        {
            if (_expandingTileGrid == null)
                return;

            _expandingTileGrid.onGridResized -= OnGridResized;
        }

        private void OnGridResized()
        {
            UpdateGridTransformSize();
            RepositionAllTileUIs();
            ResizeContentRect();
        }

        private void ResizeContentRect()
        {
            contentRect.sizeDelta = GridTransform.sizeDelta = (100f / gridImage.sprite.pixelsPerUnit)
                * (14f * ((Vector2)_expandingTileGrid.GridSize) + Vector2.one);
        }

        private void UpdateGridTransformSize()
        {
            if (gridImage == null || gridImage.sprite == null)
                return;

            GridTransform.sizeDelta = (100f / gridImage.sprite.pixelsPerUnit)
                * (14f * ((Vector2)_expandingTileGrid.GridSize) + Vector2.one);
        }

        private void RepositionAllTileUIs()
        {
            foreach (var tileUI in tileUIs)
            {
                tileUI.RectTransform.position = TileRectPositionAtVector2IntPosition(tileUI, tileUI.BasePlacedPosition);
            }
        }
    }

}

