using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DescriptionUI : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text mainDescriptionText;

    [SerializeField] private readonly List<SubDescriptionUI> subDescriptionsUIs = new List<SubDescriptionUI>();

    [SerializeField] private Transform subDescriptionHolder;

    public void UpdateUI(IDescriptable descriptable)
    {
        titleText.text = descriptable.Title;
        mainDescriptionText.text = descriptable.MainDescription;

        subDescriptionsUIs.DestroyAndClearList();

        foreach (var kvp in descriptable.SubDescriptionPrefabsAndParams)
        {
            SubDescriptionUI prefab = kvp.Key;
            object[] paramObjects = kvp.Value;

            SubDescriptionUI newSubDescription = Instantiate(prefab, subDescriptionHolder);
            newSubDescription.UpdateUI(paramObjects);

            subDescriptionsUIs.Add(newSubDescription);
        }
    }
}
