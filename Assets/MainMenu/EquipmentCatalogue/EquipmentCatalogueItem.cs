using System.Collections;
using System.Collections.Generic;
using Units;
using UnityEngine;
using UnityEngine.UI;

namespace Equipment
{
    public class EquipmentCatalogueItem : MonoBehaviour
    {
        public EquipmentSO Equipment { private set; get; }

        [SerializeField]
        private Text equipmentNameText;
        [SerializeField]
        private Text equipmentDescriptionText;
        [SerializeField]
        private Image equipmentGraphics;

        [SerializeField]
        private Text equipmentLevelText;

        [SerializeField]
        private Transform equipmentBuffHolder;
        [SerializeField]
        private EquipmentBuffItemUI equipmentBuffUIPrefab;

        private List<EquipmentBuffItemUI> equipmentBuffUIItems = new List<EquipmentBuffItemUI>();

        [SerializeField]
        private Text upgradeAmountText;

        [SerializeField]
        private Text unlockOrUpgradeText;

        [SerializeField]
        private Button unlockOrUpgradeButton;

        public void Setup(EquipmentSO equipment)
        {
            this.Equipment = equipment;
            LockedLevelState state = EquipmentManager.Instance.EquipmentStates[equipment];

            equipmentNameText.text = (state == LockedLevelState.Unknown) ? "???" : equipment.name;
            equipmentDescriptionText.text = (state == LockedLevelState.Unknown) ? "?????" : equipment.description;

            equipmentGraphics.sprite = equipment.sprite;
            equipmentGraphics.color = (state == LockedLevelState.Unknown) ? Color.black : Color.white;
            equipmentGraphics.SetNativeSize();

            unlockOrUpgradeText.text = (state == LockedLevelState.Unknown) ? "?" : "Upgrade";

            foreach (EquipmentBuffItemUI buffUI in equipmentBuffUIItems)
            {
                Destroy(buffUI.gameObject);
            }
            equipmentBuffUIItems.Clear();

            foreach (var equipmentBuff in equipment.EquipmentStatsDictionary)
            {
                EquipmentBuffItemUI buffUI = Instantiate(equipmentBuffUIPrefab, equipmentBuffHolder);
                buffUI.Init(equipmentBuff.Value, equipmentBuff.Key);
                equipmentBuffUIItems.Add(buffUI);
            }

            switch (state)
            {
                case LockedLevelState.Unknown:
                    upgradeAmountText.text = "???";
                    unlockOrUpgradeText.text = "Unknown";
                    equipmentLevelText.text = "Lvl ?";
                    break;
                case LockedLevelState.Level3:
                    upgradeAmountText.text = "Max";
                    unlockOrUpgradeText.text = "Max";
                    equipmentLevelText.text = "Max";
                    break;
                default:
                    upgradeAmountText.text = "";
                    unlockOrUpgradeText.text = "";
                    equipmentLevelText.text = "";
                    break;
            }

            unlockOrUpgradeButton.interactable = state != LockedLevelState.Unknown && state != LockedLevelState.Level3;
        }
    }

}



