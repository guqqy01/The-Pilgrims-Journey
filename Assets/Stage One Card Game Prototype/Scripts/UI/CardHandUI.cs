using System.Collections.Generic;
using TPJ.Gameplay;
using UnityEngine;

namespace TPJ.UI
{
    public class CardHandUI : MonoBehaviour
    {
        [SerializeField] private RectTransform handArea;
        [SerializeField] private CardHandUILayout layout;
        [SerializeField] private CardWidgetUI cardUIPrefab;
        [SerializeField] private DeckHolder deckHolder;

        [Header("Hover")]
        [SerializeField] private float hoverYOffset = 80f;
        [SerializeField] private float hoverScaleBoost = 1.15f;
        [Tooltip("How far immediate neighbors of the hovered card are pushed sideways to stay visible.")]
        [SerializeField] private float hoverNeighborPushAmount = 70f;
        [Tooltip("How many cards on each side of the hovered card are pushed, fading to no push at the edge of this range.")]
        [SerializeField] private int hoverNeighborPushRange = 2;

        [Header("Drag to play")]
        [SerializeField] private RectTransform playDropZone;
        [SerializeField] private Canvas parentCanvas;

        private readonly List<CardWidgetUI> _cardUIs = new List<CardWidgetUI>();

        private CardWidgetUI _hoveredCard;
        private CardWidgetUI _draggingCard;

        private Camera EventCamera =>
            parentCanvas == null || parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : parentCanvas.worldCamera;

        private void Awake()
        {
            if (deckHolder != null)
            {
                deckHolder.OnDiscard += OnDiscard;
                deckHolder.OnDraw += OnDraw;
                deckHolder.OnPlay += OnPlay;
            }

            CardUIEventBus.OnCardHoveredSwitch += OnHoverSwitch;
            CardUIEventBus.OnCardBeginDrag += OnCardBeginDrag;
            CardUIEventBus.OnCardDragging += OnCardDragging;
            CardUIEventBus.OnCardEndDrag += OnCardEndDrag;
        }

        private void OnDestroy()
        {
            if (deckHolder != null)
            {
                deckHolder.OnDiscard -= OnDiscard;
                deckHolder.OnDraw -= OnDraw;
                deckHolder.OnPlay -= OnPlay;
            }

            CardUIEventBus.OnCardHoveredSwitch -= OnHoverSwitch;
            CardUIEventBus.OnCardBeginDrag -= OnCardBeginDrag;
            CardUIEventBus.OnCardDragging -= OnCardDragging;
            CardUIEventBus.OnCardEndDrag -= OnCardEndDrag;
        }

        private void Update()
        {
            HandleRemoveNull();
            UpdateLayout();
        }

        private void HandleRemoveNull() => _cardUIs.RemoveAll(c => c == null);

        private void OnDiscard(CardBase card)
        {
            CardWidgetUI cardToRemove = null;

            foreach (CardWidgetUI cardUI in _cardUIs)
            {
                if (cardUI.Card != card)
                    continue;

                cardToRemove = cardUI;
                break;
            }

            if (cardToRemove == null)
                return;

            if (_draggingCard == cardToRemove) _draggingCard = null;
            if (_hoveredCard == cardToRemove) _hoveredCard = null;

            _cardUIs.Remove(cardToRemove);
            Destroy(cardToRemove.gameObject);
        }

        private void UpdateLayout()
        {
            int count = _cardUIs.Count;
            if (count == 0)
                return;

            float cardWidth = GetCardWidth();
            Rect areaRect = handArea.rect;
            float availableWidth = areaRect.width;
            Vector2 centeringOffset = GetCenteringOffset(areaRect);
            float effectiveSpacing = layout.GetSpacing(count, availableWidth, cardWidth);
            int hoveredIndex = _hoveredCard != null ? _cardUIs.IndexOf(_hoveredCard) : -1;

            for (int i = 0; i < _cardUIs.Count; i++)
            {
                CardWidgetUI cardUI = _cardUIs[i];

                if (cardUI == _draggingCard)
                    continue;

                float rel = i - ((count - 1) / 2f);
                if (float.IsNaN(rel))
                    continue;

                layout.Evaluate(rel, effectiveSpacing, out var pos, out var rot, out var scale);
                pos += centeringOffset;

                if (cardUI == _hoveredCard)
                {
                    pos.y += hoverYOffset;
                    scale *= hoverScaleBoost;
                }
                else if (hoveredIndex >= 0)
                {
                    pos.x += GetHoverNeighborPush(i - hoveredIndex);
                }

                cardUI.Apply(pos, rot, scale);
            }
        }

        /// <summary>
        /// Extra horizontal push applied to cards near the hovered card so the hovered card's
        /// scale-up doesn't hide them. Full push on the immediate neighbor, fading to zero at
        /// hoverNeighborPushRange cards away.
        /// </summary>
        private float GetHoverNeighborPush(int distanceFromHovered)
        {
            int absDistance = Mathf.Abs(distanceFromHovered);
            if (absDistance == 0 || absDistance > hoverNeighborPushRange)
                return 0f;

            float t = 1f - (absDistance - 1) / (float)hoverNeighborPushRange;
            float push = hoverNeighborPushAmount * t;

            return distanceFromHovered > 0 ? push : -push;
        }

        /// <summary>
        /// Card width in handArea's local units, used to fit the hand within handArea.
        /// Prefers an actual instantiated card's rect; falls back to the prefab's rect
        /// if no cards exist yet.
        /// </summary>
        private float GetCardWidth()
        {
            if (_cardUIs.Count > 0 && _cardUIs[0] != null)
                return _cardUIs[0].RectTransform.rect.width;

            return cardUIPrefab != null ? ((RectTransform)cardUIPrefab.transform).rect.width : 1f;
        }

        /// <summary>
        /// Where a card's anchor point (per its anchorMin) actually falls within areaRect.
        /// anchoredPosition is measured relative to this point, not from the parent's pivot
        /// or center, so both centering and drag-to-cursor placement need to correct for it.
        /// Assumes the card uses a "point" anchor (anchorMin == anchorMax), which is standard
        /// for a freely-positioned, draggable UI element.
        /// </summary>
        private Vector2 GetCardAnchorPoint(Rect areaRect)
        {
            RectTransform sample = _cardUIs.Count > 0 && _cardUIs[0] != null
                ? _cardUIs[0].RectTransform
                : (cardUIPrefab != null ? (RectTransform)cardUIPrefab.transform : null);

            if (sample == null)
                return areaRect.center;

            Vector2 anchorMin = sample.anchorMin;
            return new Vector2(
                Mathf.Lerp(areaRect.xMin, areaRect.xMax, anchorMin.x),
                Mathf.Lerp(areaRect.yMin, areaRect.yMax, anchorMin.y));
        }

        /// <summary>Offset to add to a layout-relative position so that it lands centered within areaRect.</summary>
        private Vector2 GetCenteringOffset(Rect areaRect) => areaRect.center - GetCardAnchorPoint(areaRect);

        private void OnPlay(CardBase card) { }

        private void OnDraw(CardPile cardPile)
        {
            foreach (CardBase card in cardPile)
            {
                CardWidgetUI cardUI = Instantiate(cardUIPrefab, handArea);
                cardUI.Setup(card);
                _cardUIs.Add(cardUI);
            }
        }

        private void SetIndecies(CardWidgetUI hoveredCard)
        {
            var depthList = new List<(CardWidgetUI card, int depth)>();

            int hoveredCardIndex = _cardUIs.IndexOf(hoveredCard);
            if (hoveredCardIndex < 0)
                return;

            int depth = 0;
            for (int i = hoveredCardIndex; i < _cardUIs.Count; i++)
            {
                depthList.Add((_cardUIs[i], depth));
                depth++;
            }

            depth = 1;
            for (int i = hoveredCardIndex - 1; i >= 0; i--)
            {
                depthList.Add((_cardUIs[i], depth));
                depth++;
            }

            depthList.Sort((a, b) => a.depth.CompareTo(b.depth));
            depthList.Reverse();

            for (int i = 0; i < depthList.Count; i++)
            {
                depthList[i].card.transform.SetSiblingIndex(i);
            }
        }

        public void OnHoverSwitch(CardWidgetUI card, bool hovering)
        {
            if (_draggingCard != null)
                return;

            if (hovering)
            {
                SetIndecies(card);
                _hoveredCard = card;
            }
            else if (_hoveredCard == card)
            {
                _hoveredCard = null;
            }
        }

        private void OnCardBeginDrag(CardWidgetUI card)
        {
            if (!_cardUIs.Contains(card))
                return;

            _draggingCard = card;
            if (_hoveredCard == card)
                _hoveredCard = null;

            card.transform.SetAsLastSibling();
        }

        private void OnCardDragging(CardWidgetUI card, Vector2 screenPosition)
        {
            if (card != _draggingCard)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(handArea, screenPosition, EventCamera, out Vector2 localPoint))
                card.RectTransform.anchoredPosition = localPoint - GetCardAnchorPoint(handArea.rect);

            ReorderAroundDraggedCard();
        }

        private void ReorderAroundDraggedCard()
        {
            float draggedX = _draggingCard.RectTransform.anchoredPosition.x;

            int newIndex = 0;
            foreach (CardWidgetUI other in _cardUIs)
            {
                if (other == _draggingCard)
                    continue;

                if (other.RectTransform.anchoredPosition.x < draggedX)
                    newIndex++;
            }

            int currentIndex = _cardUIs.IndexOf(_draggingCard);
            if (newIndex == currentIndex)
                return;

            _cardUIs.RemoveAt(currentIndex);
            _cardUIs.Insert(newIndex, _draggingCard);
        }

        private void OnCardEndDrag(CardWidgetUI card, Vector2 screenPosition)
        {
            if (card != _draggingCard)
                return;

            SetIndecies(card);

            _draggingCard = null;

            if (playDropZone != null &&
                RectTransformUtility.RectangleContainsScreenPoint(playDropZone, screenPosition, EventCamera))
            {
                CardUIEventBus.RaiseCardDroppedForPlay(card.Card, screenPosition);
            }
        }
    }
}