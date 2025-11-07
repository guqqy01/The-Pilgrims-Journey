using Equipment;
using System.Collections.Generic;
using UnityEngine;
using Units.UI;

namespace Equipment.UI
{
    public class EquipmentInfoDisplay : InfoDisplay
    {
        [SerializeField]
        private UnitStatSubDescriptionUI StatDecriptionPrefab;

        private List<UnitStatSubDescriptionUI> statDescriptions = new List<UnitStatSubDescriptionUI>();

        [SerializeField]
        private Transform statDescriptionParent;

        protected override void UpdateUI(IInfo selectedInfo)
        {
            if (selectedInfo is not DragableEquipmentUI && selectedInfo is not EquipmentSO && selectedInfo is not null)
                return;

            statDescriptions.DestroyAndClearList();

            base.UpdateUI(selectedInfo);

            if (selectedInfo is DragableEquipmentUI equipmentUI && equipmentUI.EquipmentSO != null)
            {
                EquipmentSO equipmentSO = equipmentUI.EquipmentSO;

                foreach (var buff in equipmentSO.EquipmentStatsDictionary)
                {
                    UnitStatSubDescriptionUI descriptionStatUI = Instantiate(StatDecriptionPrefab, statDescriptionParent);
                    descriptionStatUI.Setup(buff.Value, buff.Key);
                    statDescriptions.Add(descriptionStatUI);
                }

                return;
            }

            if (selectedInfo is EquipmentSO equipmentSO2)
            {
                foreach (var buff in equipmentSO2.EquipmentStatsDictionary)
                {
                    UnitStatSubDescriptionUI descriptionStatUI = Instantiate(StatDecriptionPrefab, statDescriptionParent);
                    descriptionStatUI.Setup(buff.Value, buff.Key);
                    statDescriptions.Add(descriptionStatUI);
                }
            }
        }
    }

}

