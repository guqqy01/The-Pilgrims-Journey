using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenuItem : MonoBehaviour
{
    private Upgrade upgrade;

    public Text upgradeNameText;
    public Text upgradeDescription;

    public Transform levelSquaresHolder;
    public Sprite emptySquare;
    public Sprite filledSquare;

    private List<Image> levelSquares = new List<Image>();

    [SerializeField]
    private Image squareImagePrefab;

    [SerializeField]
    private Button button;
    [SerializeField]
    private Text buttonText;

    public void Init(Upgrade upgrade)
    {
        this.upgrade = upgrade;
        upgradeNameText.text = upgrade.stat + " " + upgrade.buffType;

        string currentSign = "";

        if(upgrade.buffType == BuffType.Multiplier)
        {
            currentSign = "x";
        }
        else if((upgrade.Value is float floatValue && floatValue >= 0f) || (upgrade.Value is int intValue && intValue >= 0))
        {
            currentSign = "+";
        }

        string nextSign = "";

        if (upgrade.buffType == BuffType.Multiplier)
        {
            nextSign = "x";
        }
        else if((upgrade.NextValue is float floatValue && floatValue >= 0f) || (upgrade.NextValue is int intValue && intValue >= 0))
        {
            nextSign = "+";
        }

        string upgradeDescriptionString = "";
        if (upgrade.Level < 5)
            upgradeDescriptionString = "Current " + currentSign + upgrade.Value.ToString() + " -> " + nextSign + upgrade.NextValue.ToString();
        else
            upgradeDescriptionString = currentSign + upgrade.Value.ToString() + " Max";

        upgradeDescription.text = upgradeDescriptionString;

        for (int i = 0; i < levelSquares.Count; i++)
        {
            if (levelSquares[i] != null)
                Destroy(levelSquares[i].gameObject);
        }

        levelSquares.Clear();

        for (int i = 0; i <  upgrade.Values.Length; i++)
        {
            Image squareImage = Instantiate(squareImagePrefab, levelSquaresHolder);

            squareImage.sprite = upgrade.Level > i ? filledSquare : emptySquare;

            levelSquares.Add(squareImage);
        }

        button.interactable = upgrade.Level < 5 && ResourceManager.Instance.UpgradePoints > 0;
        buttonText.text = (upgrade.Level < 5) ? "Upgrade" : "Max";
    }

    public void UpgradeButtonPress()
    {
        upgrade.TryLevelUp();
        
        Init(upgrade);
    }
}
