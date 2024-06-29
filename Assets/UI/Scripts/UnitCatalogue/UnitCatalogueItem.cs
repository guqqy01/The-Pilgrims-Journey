using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;

public class UnitCatalogueItem : MonoBehaviour
{
    public UnitSO Unit { private set; get; }

    [SerializeField]
    private Text unitNameText;
    [SerializeField]
    private Text unitDescriptionText;
    [SerializeField]
    private Image unitGraphics;

    [SerializeField] 
    private Text unitLevelText;

    [SerializeField]
    private Text spawnRateText;
    [SerializeField]
    private Text movementSpeedText;
    [SerializeField]
    private Text healthText;
    [SerializeField]
    private Text shieldText;
    [SerializeField]
    private Text shieldRegenerationText;
    [SerializeField]
    private Text shieldCooldownText;

    [SerializeField]
    private Text upgradeAmountText;

    [SerializeField]
    private Text unlockOrUpgradeText;

    [SerializeField]
    private Button unlockOrUpgradeButton;

    public void Setup(UnitSO unit)
    {
        this.Unit = unit;
        LockedLevelState state = UnitManager.Instance.UnitStates[unit];

        unitNameText.text = (state == LockedLevelState.Unknown) ? "???" : unit.name;
        unitDescriptionText.text = (state == LockedLevelState.Unknown) ? "?????" : unit.Description;

        unitGraphics.sprite = unit.sprite;
        unitGraphics.color = (state == LockedLevelState.Unknown) ? Color.black : Color.white;

        spawnRateText.text = "Spawn Rate: " + ((state == LockedLevelState.Unknown) ? "?" : unit.SpawnRate.ToString());
        movementSpeedText.text = "Movement Speed: " + ((state == LockedLevelState.Unknown) ? "?" : unit.MovementSpeed.ToString());
        healthText.text = "Health: " + ((state == LockedLevelState.Unknown) ? "?" : unit.Health.ToString());
        shieldText.text = "Shield: " + ((state == LockedLevelState.Unknown) ? "?" : unit.Shield.ToString());
        shieldRegenerationText.text = "Shield Regeneration: " + ((state == LockedLevelState.Unknown) ? "?" : unit.ShieldRegeneration.ToString());
        shieldCooldownText.text = "Shield Cooldown: " + ((state == LockedLevelState.Unknown) ? "?" : unit.ShieldCooldown.ToString());

        unlockOrUpgradeText.text = (state == LockedLevelState.Unknown) ? "?" : "Upgrade";

        switch (state)
        {
            case LockedLevelState.Unknown:
                upgradeAmountText.text = "???";
                unlockOrUpgradeText.text = "Unknown";
                unitLevelText.text = "Lvl ?";
                break;
            case LockedLevelState.Level3:
                upgradeAmountText.text = "Max";
                unlockOrUpgradeText.text = "Max";
                unitLevelText.text = "Max";
                break;
            default:
                upgradeAmountText.text = "";
                unlockOrUpgradeText.text = "";
                unitLevelText.text = "";
                break;
        }

        unlockOrUpgradeButton.interactable = state != LockedLevelState.Unknown && state != LockedLevelState.Level3;
    }

    public void UpgradeUnit()
    {
        if (Unit == null)
            return;

        LockedLevelState state = UnitManager.Instance.UnitStates[Unit];

        if (state == LockedLevelState.Unknown || state == LockedLevelState.Level3)
            return;

        UnitManager.Instance.ChangeUnitState(Unit, state + 1);
    }
}
