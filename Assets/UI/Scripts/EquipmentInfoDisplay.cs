using Equipment;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentInfoDisplay : InfoDisplay
{
    [SerializeField]
    private DescriptionStatUI StatDecriptionPrefab;

    private List<DescriptionStatUI> statDescriptions = new List<DescriptionStatUI>();

    [SerializeField]
    private Transform statDescriptionParent;

    protected override void UpdateUI(IInfo selectedInfo)
    {
        if(selectedInfo is not Equipment.DragableEquipmentUI && selectedInfo is not Equipment.EquipmentSO && selectedInfo is not null) 
            return;

        statDescriptions.DestroyAndClearList();

        base.UpdateUI(selectedInfo);

        if(selectedInfo is Equipment.DragableEquipmentUI equipmentUI && equipmentUI.EquipmentSO != null)
        {
            EquipmentSO equipmentSO = equipmentUI.EquipmentSO;

            foreach(var buff in equipmentSO.EquipmentStatsDictionary)
            {
                DescriptionStatUI descriptionStatUI = Instantiate(StatDecriptionPrefab, statDescriptionParent);
                descriptionStatUI.Setup(buff.Value, buff.Key);
                statDescriptions.Add(descriptionStatUI);
            }

            return;
        }

        if(selectedInfo is Equipment.EquipmentSO equipmentSO2)
        {
            foreach (var buff in equipmentSO2.EquipmentStatsDictionary)
            {
                DescriptionStatUI descriptionStatUI = Instantiate(StatDecriptionPrefab, statDescriptionParent);
                descriptionStatUI.Setup(buff.Value, buff.Key);
                statDescriptions.Add(descriptionStatUI);
            }
        }
    }
}
