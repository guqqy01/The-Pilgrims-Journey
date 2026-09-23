using TMPro;
using TPJ.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TPJ.UI
{
    public class CardWidgetUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _rect;
        public RectTransform RectTransform => _rect;

        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float spriteSize = 30f;
        [SerializeField] private float followSpeed = 12f;

        [SerializeField] private PointHolderUI pointHolderUI;
        [SerializeField] private CardTypeUIHolder cardTypeUIHolder;
        private CardBase _card;
        public CardBase Card => _card;

        private bool _interactable = true;
        private bool _dragging;

        private Vector2 _targetPos;
        private Quaternion _targetRot = Quaternion.identity;
        private float _targetScale = 1f;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _targetPos = _rect.anchoredPosition;
        }

        private void Update()
        {
            float t = Time.deltaTime * followSpeed;
            _rect.localRotation = Quaternion.Slerp(_rect.localRotation, _targetRot, t);

            if (_dragging)
                return;

            _rect.anchoredPosition = Vector2.Lerp(_rect.anchoredPosition, _targetPos, t);
            _rect.localScale = Vector3.Lerp(_rect.localScale, Vector3.one * _targetScale, t);
        }

        public void Setup(CardBase card)
        {
            _card = card;
            IDescriptable descriptable = card.GetDisplayInfo();
            itemImage.sprite = descriptable.Sprite;
            itemImage.NativeRatioScaleToSize(spriteSize);
            title.text = descriptable.Title;
            description.text = descriptable.ShortDescription;

            pointHolderUI.CreatePointUIs(card.CardActionCost, card.CardActionCost);

            if(card is ITileCardType tileCardTypeInterface)
            {
                cardTypeUIHolder.Setup(tileCardTypeInterface.TileCardType);
            }

            GetComponent<CardHighlighterUI>()?.Setup(card);
        }

        /// <summary>Sets the layout-driven target transform; CardWidgetUI eases toward it each frame.</summary>
        public void Apply(Vector2 pos, Quaternion rot, float scale)
        {
            if (_dragging)
            {
                if (_targetRot != Quaternion.identity)
                    _targetRot = Quaternion.identity;
                return;
            }

            _targetPos = pos;
            _targetRot = rot;
            _targetScale = scale;
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            if (canvasGroup != null)
                canvasGroup.alpha = interactable ? 1f : 0.4f;
        }

        public void OnPointerEnter(PointerEventData eventData) => CardUIEventBus.RaiseCardHoveredSwitch(this, true);
        public void OnPointerExit(PointerEventData eventData) => CardUIEventBus.RaiseCardHoveredSwitch(this, false);

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_interactable || eventData.dragging)
                return;

            CardUIEventBus.RaiseCardSelected(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_interactable)
                return;

            _dragging = true;
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;

            CardUIEventBus.RaiseCardHoveredSwitch(this, false);
            CardUIEventBus.RaiseCardBeginDrag(this);

            _targetRot = Quaternion.identity;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            CardUIEventBus.RaiseCardDragging(this, eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            _dragging = false;
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = true;

            CardUIEventBus.RaiseCardEndDrag(this, eventData.position);
        }
    }
}