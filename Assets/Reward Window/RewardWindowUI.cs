using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

public class RewardWindowUI : MonoBehaviour
{
    [SerializeField] private RewardItemUI rewardItemUIPrefab;

    [SerializeField] private readonly List<RewardItemUI> rewardItemUIs = new List<RewardItemUI>();

    [SerializeField] private DescriptionUI descriptionText;

    public void Setup()
    {

    }

    public void ShowItemDescription()
    {

    }

    public void Select()
    {
       
    }
}

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

public interface IDescriptable
{
    public string Title { get; }

    public string MainDescription { get; }

    public Dictionary<SubDescriptionUI, object[]> SubDescriptionPrefabsAndParams { get; }
}


public abstract class SubDescriptionUI : MonoBehaviour
{
    public abstract void UpdateUI(params object[] obj);
}
