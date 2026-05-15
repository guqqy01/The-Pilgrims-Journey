using Units;
using UnityEngine;
namespace Tiles
{
    public class UnitExpandingTileGridUI : ExpandingTileGridUI<UnitExpandingTileGrid, UnitSO>
    {
        [SerializeField]
        private RectTransform contenetRect;

        private void Awake()
        {
            if (_expandingTileGrid != null)
                _expandingTileGrid.onGridResized += OnGridResized;
        }

        private void OnDestroy()
        {
            if(_expandingTileGrid != null)
                _expandingTileGrid.onGridResized -= OnGridResized;
        }

        private void OnGridResized()
        {
            contenetRect.sizeDelta = GridTransform.sizeDelta = (100f / gridImage.sprite.pixelsPerUnit)
                * (14f * ((Vector2)_expandingTileGrid.GridSize) + Vector2.one);
        }
    }

}

