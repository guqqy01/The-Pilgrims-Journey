using UnityEngine;
using System;
namespace UI.CardCarousel
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class CarouselCard : MonoBehaviour
    {
        private RectTransform rect;
        private CanvasGroup canvasGroup;

        public Action<CarouselCard> onButtonClick;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void ButtonClick()
        {
            onButtonClick?.Invoke(this);
        }

        private void OnDestroy()
        {
            onButtonClick = null;
        }

        public void Apply(Vector2 pos, Quaternion rot, float scale, float alpha)
        {
            rect.anchoredPosition = pos;
            rect.localScale = Vector3.one * scale;
            rect.localRotation = rot;

            canvasGroup.alpha = alpha;
        }
    }
}

