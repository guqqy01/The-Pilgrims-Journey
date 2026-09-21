using Tiles;
using Tiles.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TPJ.UI
{
    public class HologramTileUI : TileUIBase
    {
        [SerializeField]
        private TileUIStyle _tileUIStyle;
        public override TileUIStyle TileUIStyle => _tileUIStyle;

        [SerializeField]
        private Image objectImage;

        // The hologram is entirely driven by CardPlacementController, not by
        // TileUIBase's own drag/rotate/cancel loop.
        protected override bool SuppressAutoUpdate => true;

        /// <summary>Copies visual style from the real prefab this tile would use once placed,
        /// and reparents onto the top-level canvas so it renders above everything else.</summary>
        public void Setup(TileUIBase tileUIBase)
        {
            _tileUIStyle = tileUIBase.TileUIStyle;
            RectTransform.SetParent(TileUIManager.Canvas.transform, true);
        }

        public override void Rotate(int angle)
        {
            base.Rotate(angle);
            objectImage.rectTransform.rotation = Quaternion.Euler(Vector3.zero);
        }

        public override void RotateTo(float angle)
        {
            base.RotateTo(angle);
            objectImage.rectTransform.rotation = Quaternion.Euler(Vector3.zero);
        }

        /// <summary>Assigns the grid this hologram is currently sized/previewed against,
        /// so ApplyGridSizing/rotation-driven resizing actually runs. Pass null while the
        /// hologram is off-grid (click-to-place before a grid target is known).</summary>
        public void SetGridContext(TileGridUI grid)
        {
            tileGridUI = grid;
            if (grid != null)
                ApplyGridSizing(grid);
        }

        private void SizeChange(TileGridUI tileGridUI)
        {
            objectImage.rectTransform.anchoredPosition = GetCenteredAnchoredPositionPositionInShape(Tile.Shape, tileGridUI);

            if (tileGridUI != null)
            {
                objectImage.NativeRatioScaleToSize(tileGridUI.SingleTileSquareSize.Min().Percent(80));
                return;
            }

            objectImage.NativeRatioScaleToSize((BaseSize / new Vector2(Tile.Width, Tile.Height)).Min().Percent(80));
        }

        public override void BaseInit(TileBase tile)
        {
            base.BaseInit(tile);
            objectImage.sprite = tile.TiledObject.Sprite;
            SizeChange(tileGridUI);
        }

        protected override void OnTileSizeChanged(TileGridUI grid)
        {
            SizeChange(grid);
        }
    }
}