using Equipment;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            UpdateStats();
            tile.onStatsChange = UpdateStats;

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
            
            Setup(tile.Equipment);
        }


        public void UpdateStats()
        {
            containers[0].Active = tile.Level >= 1;
            containers[1].Active = tile.Level >= 3;
            containers[2].Active = tile.Level >= 5;

            spawnRateText.text = tile.SpawnRateValue.ToString();
            movement.text = tile.MovementValue.ToString();
            health.text = tile.HealthValue.ToString();
            shield.text = tile.ShieldValue.ToString();
            shieldRegen.text = tile.ShieldRegenerationValue.ToString();
            shieldCooldown.text = tile.ShieldCooldownValue.ToString();

        }

        public void DeselectTile()
        {
            tile = null;

            containers[0].Deactivate();
            containers[1].Deactivate();
            containers[2].Deactivate();
            
            spawnRateText.text = "NA";
            movement.text = "NA";
            health.text = "NA";
            shield.text = "NA";
            shieldRegen.text = "NA";
            shieldCooldown.text = "NA";
        }

        public void UpgradeTile()
        {
            if (tile.requiredGoldToLevel <= ResourceManager.Instance.Gold)
            {
                tile.Upgrade();

                AudioManager.Instance.PlayButtonClick();
            }
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
