using Equipment;
using GameManagerCalls;
using UnityEngine;
using TMPro;
using Units;

namespace Tiles
{
    public class TileStatsUI : EquipmentInventory, IOnSetupNewGame, IOnClear
    {
        private UnitTileUI unitTileUI;

        [SerializeField]
        private TextMeshProUGUI spawnRateText;
        [SerializeField]
        private TextMeshProUGUI movement;
        [SerializeField]
        private TextMeshProUGUI health;
        [SerializeField]
        private TextMeshProUGUI shield;
        [SerializeField]
        private TextMeshProUGUI shieldRegen;
        [SerializeField]
        private TextMeshProUGUI shieldCooldown;

        [SerializeField]
        private TextMeshProUGUI tileInfoText;

        [SerializeField]
        private TextMeshProUGUI levelText;
        [SerializeField]
        private TextMeshProUGUI totalNumberOfUnits;
        [SerializeField]
        private TextMeshProUGUI numberOfUnitsToNextLevel;

        public void TileSelected(TileUIBase tile)
        {
            if (tile is not UnitTileUI unitTileUI)
                return;

            Containers[0].onEquipmentChanged -= OnContainer0Change;
            Containers[1].onEquipmentChanged -= OnContainer1Change;
            Containers[2].onEquipmentChanged -= OnContainer2Change;
            Containers[3].onEquipmentChanged -= OnContainer3Change;

            if (tile == null)
            {
                DeselectTile();
                return;
            }

            Clear();

            this.unitTileUI = unitTileUI;

            tileInfoText.text = unitTileUI.Title;

            UpdateStats();
            UpdateLevelUnitsAndHealth();
            unitTileUI.Tile.onUpdateUI = UpdateStats;
            unitTileUI.UnitTile.onUpdateLevelUnitsAndHealthUI = UpdateLevelUnitsAndHealth;

            Containers[0].onEquipmentChanged += OnContainer0Change;
            Containers[1].onEquipmentChanged += OnContainer1Change;
            Containers[2].onEquipmentChanged += OnContainer2Change;
            Containers[3].onEquipmentChanged += OnContainer3Change;

            Setup(unitTileUI.Equipment);
        }

        private void OnContainer0Change(EquipmentSO from, EquipmentSO to)
        {
            if (unitTileUI == null) return;

            unitTileUI.Equipment[0] = to;
            UpdateStats();
        }

        private void OnContainer1Change(EquipmentSO from, EquipmentSO to)
        {
            if (unitTileUI == null) return;

            unitTileUI.Equipment[1] = to;
            UpdateStats();
        }
        private void OnContainer2Change(EquipmentSO from, EquipmentSO to)
        {
            if (unitTileUI == null) return;

            unitTileUI.Equipment[2] = to;
            UpdateStats();
        }
        private void OnContainer3Change(EquipmentSO from, EquipmentSO to)
        {
            if (unitTileUI == null) return;

            unitTileUI.Equipment[3] = to;
            UpdateStats();
        }

        public void UpdateStats()
        {
            if (unitTileUI == null)
                DeselectTile();

            for (int i = 0; i < Containers.Length; i++)
            {
                Containers[i].Active = unitTileUI.UnitTile.Level > i;
            }

            spawnRateText.text = unitTileUI.SpawnRateValue.ToString();
            movement.text = unitTileUI.MovementValue.ToString();
            health.text = unitTileUI.HealthValue.ToString();
            shield.text = unitTileUI.ShieldValue.ToString();
            shieldRegen.text = unitTileUI.ShieldRegenerationValue.ToString();
            shieldCooldown.text = unitTileUI.ShieldCooldownValue.ToString();

        }

        public void UpdateLevelUnitsAndHealth()
        {
            if (unitTileUI == null)
                return;

            totalNumberOfUnits.text = $"Total units: {unitTileUI.UnitTile.CurrentAmountOfUnitsInTile}/{unitTileUI.UnitTile.MaxAmountOfUnitsInTile}";

            if (unitTileUI.UnitTile.IsMaxLevel)
            {
                levelText.text = "Level: Max";
                numberOfUnitsToNextLevel.text = "Units to upgrade: Max Level";
            }
            else
            {
                levelText.text = $"Level: {unitTileUI.UnitTile.Level}";
                numberOfUnitsToNextLevel.text = $"Units to upgrade: {unitTileUI.UnitTile.CurrentAmountOfUnitsInTile}/{unitTileUI.UnitTile.AmountOfUnitsEscapedNeededToLevel}";
            }
        }

        public void DeselectTile()
        {
            unitTileUI = null;

            Containers[0].onEquipmentChanged -= OnContainer0Change;
            Containers[1].onEquipmentChanged -= OnContainer1Change;
            Containers[2].onEquipmentChanged -= OnContainer2Change;
            Containers[3].onEquipmentChanged -= OnContainer3Change;

            for (int i = 0; i < Containers.Length; i++)
            {
                Containers[i].Deactivate();
            }

            

            spawnRateText.text = "NA";
            movement.text = "NA";
            health.text = "NA";
            shield.text = "NA";
            shieldRegen.text = "NA";
            shieldCooldown.text = "NA";
            totalNumberOfUnits.text = "Total units: NA";
            levelText.text = "Level: NA";
            numberOfUnitsToNextLevel.text = "Units to upgrade: NA";
            tileInfoText.text = "Select a tile";
        }

        public override void Clear()
        {
            base.Clear();

            DeselectTile();
        }

        protected void Awake()
        {
            DeselectTile();

            TileUIEventBus.OnTileSelected += TileSelected;
            
        }

        protected override void OnDestroy()
        {
            DeselectTile();
            TileUIEventBus.OnTileSelected -= TileSelected;
            base.OnDestroy();
        }
    }

}
