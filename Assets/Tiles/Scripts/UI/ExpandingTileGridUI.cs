using UnityEngine;
using UnityEngine.UI;

namespace Tiles
{
    public class ExpandingTileGridUI<TExpandingTileGrid, TItem> : TileGridUI<TItem> where TExpandingTileGrid : ExpandingTileGrid<TItem>
    {
        protected TExpandingTileGrid _expandingTileGrid;

        [SerializeField]
        protected Image gridImage;

        public new void InitiateFromRootAtAwake()
        {
            base.InitiateFromRootAtAwake();

            if(tileGrid != null && tileGrid is TExpandingTileGrid expandingTileGrid)
            {
                _expandingTileGrid = expandingTileGrid;
                _expandingTileGrid.onGridResized += OnGridResized;
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
                tileUI.RectTransform.position = TileRectPositionAtVector2IntPosition(tileUI, tileUI.PlacedPosition);
            }
        }
    }

}

