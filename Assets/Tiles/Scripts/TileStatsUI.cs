using Equipment;
using GameManagerCalls;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace Tiles
{
    public class TileStatsUI : EquipmentInventory, IOnSetupNewGame, IOnClear
    {
        private Tile tile;

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

        public void TileSelected(Tile tile)
        {
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

            this.tile = tile;

            tileInfoText.text = tile.Title;

            UpdateStats();
            UpdateLevelUnitsAndHealth();
            tile.onUpdateUI = UpdateStats;
            tile.onUpdateLevelUnitsAndHealthUI = UpdateLevelUnitsAndHealth;

            Containers[0].onEquipmentChanged += OnContainer0Change;
            Containers[1].onEquipmentChanged += OnContainer1Change;
            Containers[2].onEquipmentChanged += OnContainer2Change;
            Containers[3].onEquipmentChanged += OnContainer3Change;

            Setup(tile.Equipment);
        }

        private void OnContainer0Change(EquipmentSO from, EquipmentSO to)
        {
            if (tile == null) return;

            tile.Equipment[0] = to;
            UpdateStats();
        }

        private void OnContainer1Change(EquipmentSO from, EquipmentSO to)
        {
            if (tile == null) return;

            tile.Equipment[1] = to;
            UpdateStats();
        }
        private void OnContainer2Change(EquipmentSO from, EquipmentSO to)
        {
            if (tile == null) return;

            tile.Equipment[2] = to;
            UpdateStats();
        }
        private void OnContainer3Change(EquipmentSO from, EquipmentSO to)
        {
            if (tile == null) return;

            tile.Equipment[3] = to;
            UpdateStats();
        }

        public void UpdateStats()
        {
            if (tile == null)
                DeselectTile();

            for (int i = 0; i < Containers.Length; i++)
            {
                Containers[i].Active = tile.Level > i;
            }

            spawnRateText.text = tile.SpawnRateValue.ToString();
            movement.text = tile.MovementValue.ToString();
            health.text = tile.HealthValue.ToString();
            shield.text = tile.ShieldValue.ToString();
            shieldRegen.text = tile.ShieldRegenerationValue.ToString();
            shieldCooldown.text = tile.ShieldCooldownValue.ToString();

        }

        public void UpdateLevelUnitsAndHealth()
        {
            if (tile == null)
                return;

            totalNumberOfUnits.text = $"Total units: {tile.currentAmountOfUnitsInTile}/{tile.MaxAmountOfUnitsInTile}";

            if (tile.IsMaxLevel)
            {
                levelText.text = "Level: Max";
                numberOfUnitsToNextLevel.text = "Units to upgrade: Max Level";
            }
            else
            {
                levelText.text = $"Level: {tile.Level}";
                numberOfUnitsToNextLevel.text = $"Units to upgrade: {tile.CurrentAmountOfUnitsEscaped}/{tile.AmountOfUnitsEscapedNeededToLevel}";
            }
        }

        public void DeselectTile()
        {
            tile = null;

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

            Tile.onTileSelected += TileSelected;
            
        }

        protected override void OnDestroy()
        {
            DeselectTile();
            Tile.onTileSelected -= TileSelected;
            base.OnDestroy();
        }
    }

}
