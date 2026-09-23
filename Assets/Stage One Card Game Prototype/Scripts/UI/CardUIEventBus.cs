using System;
using TPJ.Gameplay;
using UnityEngine;

namespace TPJ.UI
{
    public static class CardUIEventBus
    {
        public static event Action<CardWidgetUI> OnCardSelected;
        public static event Action<CardWidgetUI, bool> OnCardHoveredSwitch;
        public static event Action<CardWidgetUI> OnCardBeginDrag;
        public static event Action<CardWidgetUI, Vector2> OnCardDragging;
        public static event Action<CardWidgetUI, Vector2> OnCardEndDrag;
        public static event Action<CardBase, Vector2> OnCardDroppedForPlay;

        public static void RaiseCardSelected(CardWidgetUI card) => OnCardSelected?.Invoke(card);
        public static void RaiseCardHoveredSwitch(CardWidgetUI card, bool value) => OnCardHoveredSwitch?.Invoke(card, value);
        public static void RaiseCardBeginDrag(CardWidgetUI card) => OnCardBeginDrag?.Invoke(card);
        public static void RaiseCardDragging(CardWidgetUI card, Vector2 screenPos) => OnCardDragging?.Invoke(card, screenPos);
        public static void RaiseCardEndDrag(CardWidgetUI card, Vector2 screenPos) => OnCardEndDrag?.Invoke(card, screenPos);
        public static void RaiseCardDroppedForPlay(CardBase card, Vector2 screenPos) => OnCardDroppedForPlay?.Invoke(card, screenPos);

        public static void Clear()
        {
            OnCardSelected = null;
            OnCardHoveredSwitch = null;
            OnCardBeginDrag = null;
            OnCardDragging = null;
            OnCardEndDrag = null;
            OnCardDroppedForPlay = null;
        }
    }
}