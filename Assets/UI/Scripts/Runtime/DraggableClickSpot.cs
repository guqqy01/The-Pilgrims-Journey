using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace UI
{
    public class DraggableClickSpot : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public event Action<PointerEventData> OnBeginDragging;
        public event Action<PointerEventData> OnEndDragging;
        public event Action<PointerEventData> OnDragging;

        private void OnDestroy()
        {
            OnBeginDragging = null;
            OnEndDragging = null;
            OnDragging = null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragging?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragging?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragging?.Invoke(eventData);
        }
    }
}

