using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenuItem : MonoBehaviour
{
    private Upgrade upgrade;

    [Header("UI References")]
    [SerializeField] private Text upgradeNameText;
    [SerializeField] private Text upgradeDescription;
    [SerializeField] private Transform levelSquaresHolder;
    [SerializeField] private Image squareImagePrefab;
    [SerializeField] private Sprite emptySquare;
    [SerializeField] private Sprite filledSquare;
    [SerializeField] private Button button;
    [SerializeField] private Text buttonText;

    private readonly List<Image> levelSquares = new();

    public void Init(Upgrade upgrade)
    {
        this.upgrade = upgrade;

        upgradeNameText.text = $"{upgrade.stat} {upgrade.buffType}";
        UpdateDescription();
        UpdateSquares();
        UpdateButton();
    }

    private void UpdateDescription()
    {
        string currentSign = GetSign(upgrade.Value);
        string nextSign = GetSign(upgrade.NextValue);

        if (upgrade.Level < upgrade.Values.Length)
        {
            upgradeDescription.text = $"Current {currentSign}{upgrade.Value} -> {nextSign}{upgrade.NextValue}";
        }
        else
        {
            upgradeDescription.text = $"{currentSign}{upgrade.Value} Max";
        }
    }

    private void UpdateSquares()
    {
        // Clear old squares
        foreach (var square in levelSquares)
        {
            if (square != null) Destroy(square.gameObject);
        }
        levelSquares.Clear();

        // Create new squares
        for (int i = 0; i < upgrade.Values.Length; i++)
        {
            var squareImage = Instantiate(squareImagePrefab, levelSquaresHolder);
            squareImage.sprite = upgrade.Level > i ? filledSquare : emptySquare;
            levelSquares.Add(squareImage);
        }
    }

    private void UpdateButton()
    {
        bool canUpgrade = upgrade.Level < upgrade.Values.Length && ResourceManager.Instance.UpgradePoints > 0;
        button.interactable = canUpgrade;
        buttonText.text = canUpgrade ? "Upgrade" : "Max";
    }

    private string GetSign(object value)
    {
        if (upgrade.buffType == BuffType.Multiplier) return "x";

        if (value is float f && f >= 0f) return "+";
        if (value is int i && i >= 0) return "+";

        return string.Empty;
    }

    public void UpgradeButtonPress()
    {
        upgrade.TryLevelUp();
        Init(upgrade);
    }
}