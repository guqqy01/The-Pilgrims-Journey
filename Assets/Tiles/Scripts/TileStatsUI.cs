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
            if(tile == null)
            {
                DeselectTile();
                return;
            }

            foreach(EquipmentContainer container in containers)
            {
                container.onEquipmentChange = null;
            }

            Clear();

            this.tile = tile;

            tileInfoText.text = tile.Title;

            UpdateStats();
            UpdateLevelUnitsAndHealth();
            tile.onUpdateUI = UpdateStats;
            tile.onUpdateLevelUnitsAndHealthUI = UpdateLevelUnitsAndHealth;

            containers[0].onEquipmentChange = (EquipmentSO from, EquipmentSO to) =>
            {
                tile.Equipment[0] = to;
                UpdateStats();
            };
            containers[1].onEquipmentChange = (EquipmentSO from, EquipmentSO to) =>
            {
                tile.Equipment[1] = to;
                UpdateStats();
            };
            containers[2].onEquipmentChange = (EquipmentSO from, EquipmentSO to) =>
            {
                tile.Equipment[2] = to;
                UpdateStats();
            };
            containers[3].onEquipmentChange = (EquipmentSO from, EquipmentSO to) =>
            {
                tile.Equipment[3] = to;
                UpdateStats();
            };

            Setup(tile.Equipment);
        }


        public void UpdateStats()
        {
            for (int i = 0; i < containers.Count; i++)
            {
                containers[i].Active = tile.Level > i;
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

            for (int i = 0; i < containers.Count; i++)
            {
                containers[i].Deactivate();
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

            containers = GetComponentsInChildren<EquipmentContainer>().ToList();

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
