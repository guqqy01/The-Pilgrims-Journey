using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Equipment;
using UnityEngine.UI;
public class ShopItem : MonoBehaviour
{
    public EquipmentSO Equipment;

    [SerializeField]
    private Image equipmentGraphics;
    [SerializeField]
    private float graphicsSize;

    [SerializeField]
    private Text equipmentName;

    [SerializeField]
    private Text stockText;

    private Button button;

    public delegate void OnButtonPress(EquipmentSO equipment);
    public OnButtonPress onButtonPress;

    [SerializeField]
    private Color inactiveColor = Color.grey.SetAlpha(0.3f);

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(EquipmentSO equipment, int stockAmount)
    {
        Equipment = equipment;
        equipmentGraphics.sprite = equipment.sprite;
        equipmentGraphics.SetNativeSize();
        equipmentGraphics.ScaleToSize(graphicsSize);

        equipmentName.text = equipment.name;

        SetStockAmount(stockAmount);
    }

    public void SetStockAmount(int stockAmount)
    {
        equipmentGraphics.color = (stockAmount > 0) ? Color.white : inactiveColor;
        equipmentName.color = (stockAmount > 0) ? Color.white : inactiveColor;
        stockText.text = stockAmount.ToString();
        stockText.color = (stockAmount > 0) ? Color.white : inactiveColor;
    }

    public void ButtonPress()
    {
        onButtonPress?.Invoke(Equipment);
    }
}
