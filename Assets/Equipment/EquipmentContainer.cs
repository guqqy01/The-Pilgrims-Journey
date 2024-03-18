using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentContainer : MonoBehaviour
{
    private EquipmentSO equipmentSO;

    private bool active = true;
    public bool Active
    {
        get
        {
            return active;
        }
        set
        {
            cross.SetActive(!value);
            active = value;
        }
    }

    public EquipmentSO EquipmentSO
    {
        get
        {
            return equipmentSO;
        }
        set
        {
            if (value != null)
            {
                equipmentImage.sprite = value.sprite;

                equipmentImage.SetNativeSize();
            }

            equipmentImage.gameObject.SetActive(value != null);
            EquipmentSO holder = equipmentSO;
            equipmentSO = value;
            onEquipmentChange?.Invoke(holder, equipmentSO);
        }
    }

    [SerializeField]
    private Image equipmentImage;

    [SerializeField]
    private GameObject normalSlot;
    [SerializeField]
    private GameObject selectedSlot;

    [SerializeField]
    private GameObject cross;

    public delegate void OnEquipmentChange(EquipmentSO fromEquipment, EquipmentSO toEquipment);
    public OnEquipmentChange onEquipmentChange;

    public void OnClick()
    {
        if (active == false)
            return;

        UIManager.instance.EquipmentContainerSelectEvent(this);
    }

    public void Select()
    {
        equipmentImage.color = equipmentImage.color.SetAlpha(0.5f);
        normalSlot.SetActive(false);
        selectedSlot.SetActive(true);
    }

    public void Deselect()
    {
        equipmentImage.color = equipmentImage.color.SetAlpha(1f);
        normalSlot.SetActive(true);
        selectedSlot.SetActive(false);
    }
}
