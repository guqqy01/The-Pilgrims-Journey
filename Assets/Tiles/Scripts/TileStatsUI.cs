using Equipment;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Tiles
{
    public class TileStatsUI : EquipmentInventory
    {
        private Tile tile;

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
        private Text tileInfoText;

        [SerializeField]
        private Text levelText;
        [SerializeField]
        private Text totalNumberOfUnits;
        [SerializeField]
        private Text numberOfUnitsToNextLevel;

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
            tile.Equipment[0] = to;
            UpdateStats();
        }

        private void OnContainer1Change(EquipmentSO from, EquipmentSO to)
        {
            tile.Equipment[1] = to;
            UpdateStats();
        }
        private void OnContainer2Change(EquipmentSO from, EquipmentSO to)
        {
            tile.Equipment[2] = to;
            UpdateStats();
        }
        private void OnContainer3Change(EquipmentSO from, EquipmentSO to)
        {
            tile.Equipment[3] = to;
            UpdateStats();
        }

        public void UpdateStats()
        {
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
            totalNumberOfUnits.text = $"Total Number of units: {tile.currentHealth}/{tile.MaxHealth}";

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
            totalNumberOfUnits.text = "Total Number of units: NA";
            levelText.text = "Level: NA";
            numberOfUnitsToNextLevel.text = "Units to upgrade: NA";
            tileInfoText.text = "Select a tile";
        }

        protected override void Awake()
        {
            GameManager.onSetup += Setup;
            GameManager.onClear += DeselectTile;
            GameManager.onClear += Clear;

            DeselectTile();

            Tile.onTileSelected += TileSelected;
            
        }

        /*protected override void OnDisable()
        {
            GameManager.onSetup -= Setup;
            GameManager.onClear -= DeselectTile;
            GameManager.onClear -= Clear;
            Tile.onTileSelected -= TileSelected;
        }*/
    }

}
