using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class DescriptionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI mainDescriptionText;

    [SerializeField] private readonly List<SubDescriptionUI> subDescriptionsUIs = new List<SubDescriptionUI>();

    [SerializeField] private Transform subDescriptionHolder;

    [SerializeField] private LayoutContentSizerBaseParent layoutContentSizer;
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

        layoutContentSizer.UpdateAllLayoutsDeepestFirst();
    }

}
