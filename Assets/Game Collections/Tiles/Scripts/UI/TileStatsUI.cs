using Equipment;
using GameManagerCalls;
using UnityEngine;
using TMPro;
namespace Tiles.UI
{
    public class TileStatsUI : MonoBehaviour, IOnClear
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
        private TextMeshProUGUI numberOfUnitsToNextLevel;

        public void TileSelected(TileUIBase tile)
        {
            if (tile is not UnitTileUI unitTileUI)
                return;

            if (tile == null)
            {
                DeselectTile();
                return;
            }

            Clear();

            this.unitTileUI = unitTileUI;

            tileInfoText.text = unitTileUI.Title + $" - Level: {unitTileUI.UnitTile.Level}";

            UpdateStats();
            UpdateLevelUnitsAndHealth();
            unitTileUI.Tile.onUpdateUI = UpdateStats;
            unitTileUI.UnitTile.onUpdateLevelUnitsAndHealthUI = UpdateLevelUnitsAndHealth;
        }

        public void UpdateStats()
        {
            if (unitTileUI == null)
                DeselectTile();

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

            tileInfoText.text = unitTileUI.Title + $" - Level: {unitTileUI.UnitTile.Level}";

            if (unitTileUI.UnitTile.IsMaxLevel)
            {
                numberOfUnitsToNextLevel.text = "Units to upgrade: Max Level";
            }
            else
            {
                numberOfUnitsToNextLevel.text = $"Units to upgrade: {unitTileUI.UnitTile.CurrentAmountOfUnitsEscaped}/{unitTileUI.UnitTile.AmountOfUnitsEscapedNeededToLevel}";
            }
        }

        public void DeselectTile()
        {
            unitTileUI = null;

            spawnRateText.text = "NA";
            movement.text = "NA";
            health.text = "NA";
            shield.text = "NA";
            shieldRegen.text = "NA";
            shieldCooldown.text = "NA";
            numberOfUnitsToNextLevel.text = "Units to upgrade: NA";
            tileInfoText.text = "Select a tile";
        }

        public void Clear()
        {
            DeselectTile();
        }

        protected void Awake()
        {
            DeselectTile();

            TileUIEventBus.OnTileSelected += TileSelected;
            
        }

        protected void OnDestroy()
        {
            DeselectTile();
            TileUIEventBus.OnTileSelected -= TileSelected;
        }
    }

}
