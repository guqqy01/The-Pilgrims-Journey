using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TileStatsUI : MonoBehaviour
{
    private Tile tile;

    [SerializeField]
    private EquipmentContainer equipmentContainer1;
    [SerializeField]
    private EquipmentContainer equipmentContainer2;
    [SerializeField]
    private EquipmentContainer equipmentContainer3;
 
    [SerializeField]
    private Text spawnRateText;
    [SerializeField]
    private Text movement;
    [SerializeField]
    private Text health;
    [SerializeField]
    private Text shield;
    [SerializeField]
    private Text shieldRegen;
    [SerializeField]
    private Text shieldCooldown;

    [SerializeField]
    private Text upgradeGoldRequired;

    [SerializeField]
    private Text evolveDiamondsRequired;

    [SerializeField]
    private Button upgradeButton;

    [SerializeField]
    private Button evolveButton;

    [SerializeField]
    private Text levelText;

    public void TileSelected(Tile tile)
    {
        this.tile = tile;

        tile.onStatsChange = UpdateStats;

        equipmentContainer1.onEquipmentChange = (EquipmentSO from, EquipmentSO to) => tile.equipment1 = to;
        equipmentContainer2.onEquipmentChange = (EquipmentSO from, EquipmentSO to) => tile.equipment2 = to;
        equipmentContainer3.onEquipmentChange = (EquipmentSO from, EquipmentSO to) => tile.equipment3 = to;

        equipmentContainer1.onEquipmentChange += OnEquipmentChanged;
        equipmentContainer2.onEquipmentChange += OnEquipmentChanged;
        equipmentContainer3.onEquipmentChange += OnEquipmentChanged;

        equipmentContainer1.Active = true;
        equipmentContainer2.Active = true;
        equipmentContainer3.Active = true;

        equipmentContainer1.TryChangeEquipment(tile.equipment1);
        equipmentContainer2.TryChangeEquipment(tile.equipment2);
        equipmentContainer3.TryChangeEquipment(tile.equipment3);

        UpdateStats();
    }

    public void OnEquipmentChanged(EquipmentSO from, EquipmentSO to)
    {
        UpdateStats();
    }

    public void UpdateStats()
    {
        equipmentContainer1.Active = tile.Level >= 1;
        equipmentContainer2.Active = tile.Level >= 3;
        equipmentContainer3.Active = tile.Level >= 5;

        spawnRateText.text = tile.SpawnRateValue.ToString();
        movement.text = tile.MovementValue.ToString();
        health.text = tile.HealthValue.ToString();
        shield.text = tile.ShieldValue.ToString();
        shieldRegen.text = tile.ShieldRegenerationValue.ToString();
        shieldCooldown.text = tile.ShieldCooldownValue.ToString();

        upgradeGoldRequired.text = tile.requiredGoldToLevel.ToString();
        evolveDiamondsRequired.text = tile.requiredDiamondsToEvolve.ToString();

        levelText.text = "Lvl " + tile.Level.ToString();

        upgradeButton.interactable = tile.requiredGoldToLevel <= ResourceManager.Instance.Gold;
        evolveButton.interactable = tile.requiredDiamondsToEvolve <= ResourceManager.Instance.Diamonds;
    }

    public void DeselectTile()
    {
        tile = null;

        equipmentContainer1.Deactivate();
        equipmentContainer2.Deactivate();
        equipmentContainer3.Deactivate();

        spawnRateText.text = "NA";
        movement.text = "NA";
        health.text = "NA";
        shield.text = "NA";
        shieldRegen.text = "NA";
        shieldCooldown.text = "NA";

        upgradeButton.interactable = false;
        evolveButton.interactable = false;
    }

    public void UpgradeTile()
    {
        if (tile.requiredGoldToLevel <= ResourceManager.Instance.Gold)
        {
            UIManager.instance.DeselectContainer();

            tile.Upgrade();
        }
    }

    public void EvolveTile()
    {
        if (tile.requiredDiamondsToEvolve <= ResourceManager.Instance.Diamonds)
        {
            UIManager.instance.DeselectContainer();

            tile.Upgrade();
        }
    }

    private void Start()
    {
        DeselectTile();
    }
}
