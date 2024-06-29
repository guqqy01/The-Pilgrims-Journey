using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Tiles.Tile;

namespace Equipment
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DragableEquipmentUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IInfo
    {
        private Canvas canvas;
        private CanvasGroup canvasGroup;

        public RectTransform RectTransform { private set; get; }

        public EquipmentSO EquipmentSO { private set; get; }
        [HideInInspector]
        public EquipmentContainer Container;

        [SerializeField]
        private Image graphics;
        
        public bool Dragging {  get; private set; }

        public string Title => EquipmentSO.name;

        public string Description => EquipmentSO.description;

        public string GoldAmount => EquipmentSO.goldCost.ToString();

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Init(EquipmentSO equipment, EquipmentContainer container)
        {
            EquipmentSO = equipment;
            this.Container = container;

            if(RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            graphics.sprite = equipment.sprite;
            transform.SetParent(container.transform, false);
            RectTransform.position = container.GetComponent<RectTransform>().position;
            canvas = GetComponentInParent<Canvas>();
            graphics.SetNativeSize();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Dragging)
            {
                RectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Container.Active)
                return;
            Dragging = true;
            RectTransform.SetParent(canvas.transform, true);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.5f;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Dragging = false;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            RectTransform.SetParent(Container.RectTransform, false);
            RectTransform.position = Container.RectTransform.position;
        }
         
        public void SetContainer(EquipmentContainer equipmentContainer)
        {
            Container = equipmentContainer;

            RectTransform.SetParent(Container.RectTransform, false);
            RectTransform.position = Container.RectTransform.position;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            IInfo.onInfoSelected?.Invoke(this);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                EquipmentInventory priorityInventory = EquipmentManager.Instance.GetPriorityInventory(Container.EquipmentInventory, EquipmentSO);

                if(priorityInventory != null)
                {
                    if(priorityInventory.GetFreeSpot(this.EquipmentSO, out EquipmentContainer returnContainer))
                    {
                        returnContainer.SwapEquipment(Container);
                    }
                }
            }
        }
    }

}
