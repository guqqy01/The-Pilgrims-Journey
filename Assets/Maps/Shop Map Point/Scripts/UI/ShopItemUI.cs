using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Equipment;
using UnityEngine.UI;
using TMPro;
public class ShopItemUI : MonoBehaviour
{
    public ShopItem ShopItem { private set; get; }

    public EquipmentSO Equipment { private set; get; }

    [SerializeField]
    private Image equipmentGraphics;
    [SerializeField]
    private float graphicsSize;

    [SerializeField]
    private TextMeshProUGUI equipmentName;
    [SerializeField]
    private TextMeshProUGUI goldText;

    [SerializeField]
    private TextMeshProUGUI stockText;

    private Button button;

    public event System.Action<ShopItemUI> OnButtonPress;

    [SerializeField]
    private Color inactiveColor = Color.grey.SetAlpha(0.3f);

    [SerializeField]
    private DiscountText discountText;

    public float DiscountAmount { private set; get; } = 0;

    private Color activeColor = Color.white;

    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Sprite unselectedSprite, selectedSprite;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnDestroy()
    {
        if (ShopItem != null)
        {
            ShopItem.onStockAmountChange -= SetStockAmountText;
            ShopItem.onRefreshPurchasePrice -= RefreshPurchasePrice;
        }
    }

    public void Setup(ShopItem shopItem)
    {
        if (shopItem == null)
        {
            Debug.LogError($"{nameof(ShopItemUI)}: Tried to setup with a null ShopItem.");
            return;
        }

        ShopItem = shopItem;

        Equipment = shopItem.Equipment;
        equipmentGraphics.sprite = Equipment.sprite;
        equipmentGraphics.SetNativeSize();
        equipmentGraphics.ScaleToSize(graphicsSize);

        equipmentName.text = Equipment.name;

        SetStockAmountText(shopItem.StockAmount);

        DiscountAmount = shopItem.DiscountAmount;

        discountText.SetDiscount(DiscountAmount);

        goldText.text = $"{shopItem.GetPurchaseGoldAmount().ToString()}";

        shopItem.onStockAmountChange += SetStockAmountText;
        shopItem.onRefreshPurchasePrice += RefreshPurchasePrice;
    }

    public void RefreshPurchasePrice()
    {
        goldText.text = $"{ShopItem.GetPurchaseGoldAmount().ToString()}";
    }

    public void SetStockAmountText(int stockAmount)
    {
        bool isActive = stockAmount > 0;
        Color color = isActive ? activeColor : inactiveColor;

        equipmentGraphics.color = color;
        equipmentName.color = color;
        stockText.color = color;

        stockText.text = $"Qty: {stockAmount.ToString()}";
        RefreshPurchasePrice();
    }

    public void ButtonPress()
    {
        OnButtonPress?.Invoke(this);
    }

    public void Select()
    {
        backgroundImage.sprite = selectedSprite;
    }

    public void Deselect()
    {
        backgroundImage.sprite = unselectedSprite;
    }
}
