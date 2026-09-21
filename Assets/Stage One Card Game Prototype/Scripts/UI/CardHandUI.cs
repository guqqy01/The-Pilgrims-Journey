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

        private void OnDiscard(BaseCard card)
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

            for (int i = 0; i < _cardUIs.Count; i++)
            {
                CardWidgetUI cardUI = _cardUIs[i];

                if (cardUI == _draggingCard)
                    continue;

                float rel = i - ((count - 1) / 2f);
                if (float.IsNaN(rel))
                    continue;

                layout.Evaluate(rel, out var pos, out var rot, out var scale);

                if (cardUI == _hoveredCard)
                {
                    pos.y += hoverYOffset;
                    scale *= hoverScaleBoost;
                }

                cardUI.Apply(pos, rot, scale);
            }
        }

        private void OnPlay(BaseCard card) { }

        private void OnDraw(CardPile cardPile)
        {
            foreach (BaseCard card in cardPile)
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
                card.RectTransform.anchoredPosition = localPoint;

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