using System;
using System.Collections.Generic;
using Tiles;
using Tiles.UI;
using TPJ.Gameplay;

namespace TPJ.UI
{
    public class EquipmentTargetHighlighter : UnityEngine.MonoBehaviour
    {
        private readonly List<TileUIBase> _highlighted = new List<TileUIBase>();
        private Action<TileUIBase> _onTargetChosen;

        public void BeginTargeting(BaseCard card, Action<TileUIBase> onTargetChosen)
        {
            CancelTargeting();

            TileGridBase grid = CardGameManager.Instance.GetGrid;

            if (!TileGridUI.TryGetUIForGrid(grid, out TileGridUI gridUI))
                return;

            foreach (TileUIBase tileUI in gridUI.TileUIs)
            {
                if (tileUI.Tile is not IEquipmentTarget)
                    continue;

                if (TileOwnership.Get(tileUI.Tile) != card.Owner)
                    continue;

                tileUI.Highlight(true);
                _highlighted.Add(tileUI);
            }

            _onTargetChosen = onTargetChosen;
            TileUIEventBus.OnTileSelected += OnTileSelected;
        }

        private void OnTileSelected(TileUIBase tileUI)
        {
            if (tileUI == null || !_highlighted.Contains(tileUI))
                return;

            var chosen = _onTargetChosen;
            CancelTargeting();
            chosen?.Invoke(tileUI);
        }

        public void CancelTargeting()
        {
            foreach (TileUIBase tileUI in _highlighted)
            {
                if (tileUI != null)
                    tileUI.Highlight(false);
            }

            _highlighted.Clear();
            TileUIEventBus.OnTileSelected -= OnTileSelected;
            _onTargetChosen = null;
        }

        private void OnDestroy() => CancelTargeting();
    }
}