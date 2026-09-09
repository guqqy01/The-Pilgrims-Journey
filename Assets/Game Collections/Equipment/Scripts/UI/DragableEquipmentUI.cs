using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Equipment.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DragableEquipmentUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IDescriptable
    {
        private Canvas canvas;
        private Canvas Canvas
        {
            get
            {
                if(canvas == null)
                    canvas = GetComponentInParent<Canvas>();

                return canvas;
            }
        }

        private CanvasGroup canvasGroup;

        public RectTransform RectTransform { private set; get; }

        public EquipmentSO EquipmentSO { private set; get; }
        [HideInInspector]
        public EquipmentContainerUI ContainerUI;

        [SerializeField]
        private Image graphics;

        [SerializeField]
        private DragableEquipmentHighlight highlight;

        public bool Dragging {  get; private set; }

        public string Title => EquipmentSO.name;

        public string GoldAmount => EquipmentSO.goldCost.ToString();

        public string MainDescription => EquipmentSO.description;

        public Sprite Sprite => EquipmentSO.Sprite;

        public List<ISubDescriptionData> GetSubDescriptionData => EquipmentSO != null ? EquipmentSO.GetSubDescriptionData : new List<ISubDescriptionData>();

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            
            canvasGroup = GetComponent<CanvasGroup>();
        }
        

        public void Init(EquipmentSO equipment, EquipmentContainerUI container)
        {
            EquipmentSO = equipment;
            this.ContainerUI = container;

            if(RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            graphics.sprite = equipment.sprite;
            transform.SetParent(container.transform, false);
            RectTransform.position = container.GetComponent<RectTransform>().position;
            graphics.SetNativeSize();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Dragging)
            {
                RectTransform.anchoredPosition += eventData.delta / Canvas.scaleFactor;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!ContainerUI.Active)
                return;

            Info.Select(this);
            Dragging = true;
            RectTransform.SetParent(Canvas.transform, true);
            canvasGroup.blocksRaycasts = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Dragging = false;
            canvasGroup.blocksRaycasts = true;

            RectTransform.SetParent(ContainerUI.RectTransform, false);
            RectTransform.position = ContainerUI.RectTransform.position;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Info.Select(this);

            if (InputManager.Instance.Actions.UI_HeroStats.QuickMoveEquipment.IsPressed() /*Input.GetKey(KeyCode.LeftShift)*/)
            {
                EquipmentInventory priorityInventory = EquipmentInventoryUIManager.Instance.GetPriorityInventory(ContainerUI.Container.ParentInventory, EquipmentSO);

                if(priorityInventory != null)
                {
                    if(priorityInventory.GetFreeSpot(this.EquipmentSO, out EquipmentContainer returnContainer))
                    {
                        returnContainer.TrySwapEquipment(ContainerUI.Container);
                    }
                }
            }
        }

        void Select()
        {
            if (highlight == null)
                return;

            highlight.SetActive(true, EquipmentSO);
        }

        void Deselect()
        {
            if(highlight != null)
                highlight.SetActive(false);
        }
    }
}


