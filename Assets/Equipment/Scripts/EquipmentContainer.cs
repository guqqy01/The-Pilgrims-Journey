using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentContainer : MonoBehaviour
{
    public EquipmentSO AllowedEquipment;
    public EquipmentSO EquipmentSO { private set; get; }

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

    public bool TryChangeEquipment(EquipmentSO equipment)
    {
        if (!CanChangeEquipment(equipment))
            return false;

        if (equipment != null || AllowedEquipment != null)
        {
            if (equipment != null)
            {
                equipmentImage.sprite = equipment.sprite;
                equipmentImage.color = Color.white.SetAlpha(1);
            }
            else if (AllowedEquipment != null)
            {
                equipmentImage.sprite = AllowedEquipment.sprite;
                equipmentImage.color = Color.grey.SetAlpha(0.2f);
            }

            equipmentImage.SetNativeSize();
        }

        equipmentImage.gameObject.SetActive(equipment != null || AllowedEquipment != null);
        EquipmentSO holder = EquipmentSO;
        EquipmentSO = equipment;
        onEquipmentChange?.Invoke(holder, EquipmentSO);

        return true;
    }

    public bool CanChangeEquipment(EquipmentSO equipment)
    {
        if (!Active)
            return false;

        if (AllowedEquipment != null && equipment != null && AllowedEquipment != equipment)
            return false;

        return true;
    }
}
