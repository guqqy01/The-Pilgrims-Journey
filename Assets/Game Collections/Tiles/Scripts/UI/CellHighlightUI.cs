using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tiles.UI
{
    /// <summary>
    /// A single-cell highlight/selection marker. Parents itself under a TileUIBase (so it
    /// inherits that tile's position/rotation for free) and positions/sizes itself to cover
    /// exactly one grid cell of it. Used for targeting flows — like Brigade's destroy-a-
    /// section choice — where the selectable unit is one cell, not a whole tile.
    ///
    /// Two states:
    ///  - Selectable (default: yellow, clickable) — one of several candidates being offered.
    ///  - Selected (default: red, inert) — a persistent marker of what was chosen. Stays
    ///    visible until the caller destroys it once the effect actually resolves.
    ///
    /// Set up a prefab: a UI Image on a RectTransform, with this script attached and the
    /// `image` field wired. Raycast Target's starting value in the prefab doesn't matter —
    /// ShowSelectable/MarkSelected always set it explicitly.
    /// </summary>
    public class CellHighlightUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image;
        [SerializeField] private Color selectableColor = new Color(1f, 0.92f, 0.2f, 0.5f);
        [SerializeField] private Color selectedColor = new Color(0.9f, 0.15f, 0.15f, 0.7f);

        public TileUIBase TileUI { get; private set; }
        public Vector2Int LocalCell { get; private set; }
        public Vector2Int WorldCell { get; private set; }

        private Action<CellHighlightUI> _onClicked;
        private bool _isSelectable;

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>Shows this as a clickable candidate positioned over one specific cell of
        /// `tileUI`. `onClicked` fires once, when the player picks this cell.</summary>
        public void ShowSelectable(TileUIBase tileUI, Vector2Int localCell, Vector2Int worldCell, Action<CellHighlightUI> onClicked)
        {
            TileUI = tileUI;
            LocalCell = localCell;
            WorldCell = worldCell;
            _onClicked = onClicked;
            _isSelectable = true;

            Reposition();

            if (image != null)
            {
                image.color = selectableColor;
                image.raycastTarget = true;
            }

            gameObject.SetActive(true);
        }

        /// <summary>Switches this from a clickable candidate into a persistent "this is what
        /// was chosen" marker — turns red and stops intercepting clicks.</summary>
        public void MarkSelected()
        {
            _isSelectable = false;

            if (image != null)
            {
                image.color = selectedColor;
                image.raycastTarget = false;
            }
        }

        private void Reposition()
        {
            rectTransform.SetParent(TileUI.RectTransform, false);
            rectTransform.anchoredPosition = TileUI.GetAnchoredPositionForLocalCell(LocalCell);

            if (TileUI.tileGridUI != null)
            {
                rectTransform.sizeDelta = new Vector2(
                    TileUI.tileGridUI.SingleTileSquareUISizeX,
                    TileUI.tileGridUI.SingleTileSquareUISizeY);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isSelectable)
                return;

            _onClicked?.Invoke(this);
        }
    }
}