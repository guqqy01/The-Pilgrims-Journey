using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Equipment;
using UnityEngine.UI;
public class ShopItemUI : MonoBehaviour
{
    public EquipmentSO Equipment { private set; get; }

    [SerializeField]
    private Image equipmentGraphics;
    [SerializeField]
    private float graphicsSize;

    [SerializeField]
    private Text equipmentName;
    [SerializeField]
    private Text goldText;

    [SerializeField]
    private Text stockText;

    private Button button;

    public delegate void OnButtonPress(ShopItemUI shopItem);
    public OnButtonPress onButtonPress;

    [SerializeField]
    private Color inactiveColor = Color.grey.SetAlpha(0.3f);

    [SerializeField]
    private DiscountText discountText;

    public float DiscountAmount { private set; get; } = 0;

    public int FinalGoldCost => Mathf.RoundToInt(Equipment.goldCost - (Equipment.goldCost * DiscountAmount));

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(EquipmentSO equipment, int stockAmount, float discountAmount = 0)
    {
        Equipment = equipment;
        equipmentGraphics.sprite = equipment.sprite;
        equipmentGraphics.SetNativeSize();
        equipmentGraphics.ScaleToSize(graphicsSize);

        equipmentName.text = equipment.name;

        SetStockAmount(stockAmount);

        DiscountAmount = discountAmount;
        discountText.SetDiscount(discountAmount);

        goldText.text = $"{FinalGoldCost.ToString()}";
    }

    public void SetStockAmount(int stockAmount)
    {
        equipmentGraphics.color = (stockAmount > 0) ? Color.white : inactiveColor;
        equipmentName.color = (stockAmount > 0) ? Color.white : inactiveColor;
        stockText.text = $"Qty: {stockAmount.ToString()}";
        stockText.color = (stockAmount > 0) ? Color.white : inactiveColor;
    }

    public void ButtonPress()
    {
        onButtonPress?.Invoke(this);
    }
}
