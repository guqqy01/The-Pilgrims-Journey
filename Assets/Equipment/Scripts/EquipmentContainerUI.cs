using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Equipment.EquipmentContainerUI;
using static UnityEngine.Rendering.DebugUI;

namespace Equipment
{
    public class EquipmentContainerUI : MonoBehaviour, IDropHandler
    {
        public EquipmentInventoryUI ParentInventoryUI { private set; get; }
        public EquipmentContainer Container { private set; get; }

        public RectTransform RectTransform;

        [SerializeField]
        protected DragableEquipmentUI dragableEquipmentUIPrefab;

        private DragableEquipmentUI dragableEquipmentUI;

        public EquipmentSO EquipmentSO => dragableEquipmentUI != null ? dragableEquipmentUI.EquipmentSO : null;


        [SerializeField]
        private Image equipmentBackImage;

        [SerializeField]
        private GameObject cross;

        public bool Active => Container != null && Container.Active;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public void Init(EquipmentContainer container, EquipmentInventoryUI parentInventoryUI)
        {
            Container = container;
            Container.onEquipmentChanged += UpdateEquipmentChanged;
            Container.onActiveChange += UpdateActiveUI;
            Container.onAllowedEquipmentChange += UpdateAllowedEquipmentUI;

            ParentInventoryUI = parentInventoryUI;
        }

        private void UpdateActiveUI()
        {
            cross.SetActive(Container.Active);
        }

        private void UpdateAllowedEquipmentUI()
        {
            if (Container == null)
                return;

            if (Container.AllowedEquipment != null)
            {
                equipmentBackImage.sprite = Container.AllowedEquipment.sprite;
                equipmentBackImage.color = Color.grey.SetAlpha(0.3f);
            }
            equipmentBackImage.gameObject.SetActive(dragableEquipmentUI != null || Container.AllowedEquipment != null);
            equipmentBackImage.SetNativeSize();
        }

        private void UpdateEquipmentChanged(EquipmentSO fromEquipment, EquipmentSO toEquipment)
        {
            if(toEquipment == null)
            {
                Clear();
                UpdateAllowedEquipmentUI();
                return;
            }

            if(dragableEquipmentUI == null)
                dragableEquipmentUI = Instantiate(dragableEquipmentUIPrefab);

            dragableEquipmentUI.Init(toEquipment, this);
        }

        
        public void Clear()
        {
            if (dragableEquipmentUI == null)
                return;

            Destroy(dragableEquipmentUI.gameObject);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            if (!eventData.pointerDrag.TryGetComponent(out DragableEquipmentUI droppedEquipmentUI))
                return;

            if (!droppedEquipmentUI.Dragging)
                return;

            if (Container.TrySwapEquipment(droppedEquipmentUI.ContainerUI.Container))
            {

            }
            else
            {

            }
        }
    }

}
