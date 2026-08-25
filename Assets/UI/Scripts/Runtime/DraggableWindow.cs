using UnityEngine;
using UnityEngine.EventSystems;
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

            float xPos = eventData.position.x - dragOffset.x;
            float yPos = eventData.position.y - dragOffset.y;

            // World-space bounds of the draggable area
            Vector3[] areaCorners = new Vector3[4];
            draggableArea.GetWorldCorners(areaCorners); // [0]=bottom-left, [2]=top-right

            // World-space size of the window itself
            Vector3[] windowCorners = new Vector3[4];
            _windowTransform.GetWorldCorners(windowCorners);
            float halfWidth = (windowCorners[2].x - windowCorners[0].x) / 2f;
            float halfHeight = (windowCorners[2].y - windowCorners[0].y) / 2f;

            float xMin = areaCorners[0].x + halfWidth;
            float xMax = areaCorners[2].x - halfWidth;
            float yMin = areaCorners[0].y + halfHeight;
            float yMax = areaCorners[2].y - halfHeight;

            // Guard against the window being bigger than the area
            float clampedXPos = (xMin <= xMax) ? Mathf.Clamp(xPos, xMin, xMax) : (xMin + xMax) / 2f;
            float clampedYPos = (yMin <= yMax) ? Mathf.Clamp(yPos, yMin, yMax) : (yMin + yMax) / 2f;

            _windowTransform.position = new Vector2(clampedXPos, clampedYPos);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }
    }
}

