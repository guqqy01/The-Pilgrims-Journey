using Tiles.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace UI
{

    public class DraggableWindow : MonoBehaviour
    {
        [SerializeField]
        private DraggableClickSpot draggableClickSpot;

        private RectTransform _windowTransform;

        [SerializeField]
        private RectTransform draggableArea;

        private bool windowLocked = false;
        private bool isDragging = false;

        private Vector2 dragOffset;

        private void Awake()
        {
            draggableClickSpot.OnBeginDragging += OnBeginDrag;
            draggableClickSpot.OnEndDragging += OnEndDrag;
            draggableClickSpot.OnDragging += OnDrag;

            _windowTransform = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            if(draggableClickSpot != null)
            {
                draggableClickSpot.OnBeginDragging -= OnBeginDrag;
                draggableClickSpot.OnEndDragging -= OnEndDrag;
                draggableClickSpot.OnDragging -= OnDrag;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (windowLocked)
                return;
            dragOffset = eventData.position - (Vector2)_windowTransform.position;
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;

            _windowTransform.position = eventData.position - dragOffset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }
    }
}

