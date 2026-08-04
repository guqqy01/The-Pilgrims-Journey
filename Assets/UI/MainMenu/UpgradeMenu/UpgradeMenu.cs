using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
    public UpgradeMenuItem upgradeMenu1;
    public UpgradeMenuItem upgradeMenu2;
    public UpgradeMenuItem upgradeMenu3;
    public UpgradeMenuItem upgradeMenu4;

    private UpgradeManager upgradeManager;

    private int startInterger = 0;

    [SerializeField]
    private Button leftButton;
    [SerializeField]
    private Button rightButton;

    private void Start()
    {
        upgradeManager = UpgradeManager.Instance;
        UpdateMenuItems();
        upgradeManager.onUpgradePointsChange += UpdateMenuItems;
    }

    public void ButtonLeft()
    {
        startInterger = Mathf.Clamp(startInterger - 4, 0, upgradeManager.upgrades.Count - (upgradeManager.upgrades.Count % 4));
        
        UpdateMenuItems();
    }

    public void ButtonRight()
    {
        startInterger = Mathf.Clamp(startInterger + 4, 0, upgradeManager.upgrades.Count - (upgradeManager.upgrades.Count % 4));
        
        UpdateMenuItems();
    }

    public void UpdateMenuItems()
    {
        upgradeMenu1.gameObject.SetActive(startInterger < upgradeManager.upgrades.Count);
        if(startInterger < upgradeManager.upgrades.Count)
            upgradeMenu1.Init(upgradeManager.upgrades[startInterger]);
        upgradeMenu2.gameObject.SetActive(startInterger + 1 < upgradeManager.upgrades.Count);
        if (startInterger + 1 < upgradeManager.upgrades.Count)
            upgradeMenu2.Init(upgradeManager.upgrades[startInterger + 1]);
        upgradeMenu3.gameObject.SetActive(startInterger + 2 < upgradeManager.upgrades.Count);
        if (startInterger + 2 < upgradeManager.upgrades.Count)
            upgradeMenu3.Init(upgradeManager.upgrades[startInterger + 2]);
        upgradeMenu4.gameObject.SetActive(startInterger + 3 < upgradeManager.upgrades.Count);
        if (startInterger + 3 < upgradeManager.upgrades.Count)
            upgradeMenu4.Init(upgradeManager.upgrades[startInterger + 3]);

        leftButton.interactable = startInterger > 0;
        rightButton.interactable = startInterger < upgradeManager.upgrades.Count - (upgradeManager.upgrades.Count % 4);
    }
}
