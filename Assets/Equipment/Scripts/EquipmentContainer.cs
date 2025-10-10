using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Equipment
{
    public class EquipmentContainer : MonoBehaviour, IDropHandler
    {
        public RectTransform RectTransform;

        private EquipmentSO allowedEquipment;
        public EquipmentSO AllowedEquipment
        {
            get
            {
                return allowedEquipment;
            }
            set
            {
                allowedEquipment = value;

                if (allowedEquipment != null)
                {
                    if(EquipmentSO != null && EquipmentSO != allowedEquipment)
                    {
                        Clear();
                    }
                    else
                    {
                        equipmentBackImage.sprite = allowedEquipment.sprite;
                        equipmentBackImage.color = Color.grey.SetAlpha(0.3f);
                    }
                }

                equipmentBackImage.gameObject.SetActive(EquipmentUI != null || AllowedEquipment != null);
                equipmentBackImage.SetNativeSize();
            }
        }

        private DragableEquipmentUI EquipmentUI;

        public EquipmentSO EquipmentSO => EquipmentUI != null ? EquipmentUI.EquipmentSO : null;


        private bool active = true;
        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
                cross.SetActive(!value);
            }
        }

        public EquipmentInventory EquipmentInventory;

        [SerializeField]
        private Image equipmentBackImage;

        [SerializeField]
        private GameObject cross;

        public delegate void OnEquipmentChange(EquipmentSO fromEquipment, EquipmentSO toEquipment);
        public OnEquipmentChange onEquipmentChange;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public void Clear()
        {
            if (EquipmentUI == null)
                return;

            Destroy(EquipmentUI.gameObject);

            TryChangeEquipment(null);
        }

        public bool TryChangeEquipment(DragableEquipmentUI equipment)
        {
            if (!CanChangeEquipment( equipment != null ? equipment.EquipmentSO : null))
                return false;

            Sprite sprite = null;

           
            if(equipment != null && equipment.EquipmentSO != null)
            {
                sprite = equipment.EquipmentSO.sprite;
            }
            else if(allowedEquipment != null)
            {
                 sprite = allowedEquipment.sprite;
            }

            equipmentBackImage.sprite = sprite;
            equipmentBackImage.color = Color.grey.SetAlpha(0.3f);
            equipmentBackImage.SetNativeSize();
            equipmentBackImage.gameObject.SetActive(equipment != null || AllowedEquipment != null);

            if (equipment != null)
            {
                equipment.SetContainer(this);
            }

            EquipmentSO holder = EquipmentSO;
            EquipmentUI = equipment;
            
            onEquipmentChange?.Invoke(holder, EquipmentSO);

            return true;
        }

        public void Deactivate()
        {
            onEquipmentChange = null;
            Active = false;
            Clear();
            TryChangeEquipment(null);
        }

        public bool CanChangeEquipment(EquipmentSO equipment)
        {
            if (equipment == null)
                return true;

            if (!Active)
                return false;

            if (AllowedEquipment != null && AllowedEquipment != equipment)
                return false;

            return true;
        }

        public bool CanChangeEquipment(DragableEquipmentUI equipment)
        {
            return CanChangeEquipment(equipment != null ? equipment.EquipmentSO : null);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            if (!eventData.pointerDrag.TryGetComponent(out DragableEquipmentUI droppedEquipmentUI))
                return;

            if (!droppedEquipmentUI.Dragging)
                return;

            SwapEquipment(droppedEquipmentUI.Container);
        }

        public void SwapEquipment(EquipmentContainer otherContainer)
        {
            DragableEquipmentUI otherEquipment = otherContainer.EquipmentUI;
            DragableEquipmentUI previousEquipmentUI = EquipmentUI;

            if (CanChangeEquipment(otherEquipment) && otherContainer.CanChangeEquipment(previousEquipmentUI))
            {
                TryChangeEquipment(otherEquipment);
                otherContainer.TryChangeEquipment(previousEquipmentUI);
                AudioManager.Instance.PlaySelectEquipment();
            }
            else
            {
                otherEquipment.RectTransform.position = otherEquipment.Container.RectTransform.position;
                otherEquipment.RectTransform.SetParent(otherEquipment.Container.RectTransform, true);
                Debug.Log("Fail to place equipment, Sound");
            }
        }


        public ContainerSave GetSave()
        {
            return new ContainerSave(EquipmentSO, AllowedEquipment, Active);
        }
    }

}
