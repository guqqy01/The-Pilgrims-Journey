using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentContainer : MonoBehaviour
{
    private EquipmentSO equipmentSO;

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

            equipmentSO = value;
        }
    }

    [SerializeField]
    private Image equipmentImage;


    public void OnClick()
    {
        UIManager.instance.EquipmentContainerSelectEvent(this);
    }

    public void Select()
    {
        equipmentImage.color = equipmentImage.color.SetAlpha(0.5f);
    }

    public void Deselect()
    {
        equipmentImage.color = equipmentImage.color.SetAlpha(1f);
    }
}