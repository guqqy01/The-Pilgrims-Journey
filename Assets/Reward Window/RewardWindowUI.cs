using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Equipment;
using Relics;
using Random = UnityEngine.Random;
public class RewardWindowUI : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static RewardWindowUI Instance { private set; get; }

    [SerializeField] private RewardWindowItemUI rewardItemUIPrefab;

    [SerializeField] private readonly List<RewardWindowItemUI> rewardItemUIs = new List<RewardWindowItemUI>();

    [SerializeField] private RectTransform itemHolder;

    [SerializeField] private DescriptionUI descriptionText;

    private IDescriptable currentSelected;

    [SerializeField]
    private EquipmentInventory equipmentInventory;

    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        gameObject.SetActive(false);
    }

    public void Setup(List<IDescriptable> descriptables)
    {
        gameObject.SetActive(true);

        foreach(var item in rewardItemUIs)
        {
            item.onSelectItem -= ShowItemDescription;
        }

        rewardItemUIs.DestroyAndClearList();

        foreach (IDescriptable descriptable in descriptables)
        {
            RewardWindowItemUI rewardWindowItemUI = Instantiate(rewardItemUIPrefab, itemHolder);
            rewardWindowItemUI.Setup(descriptable);
            rewardWindowItemUI.onSelectItem += ShowItemDescription;
            rewardItemUIs.Add(rewardWindowItemUI);
        }

        if(rewardItemUIs.Count > 0)
            ShowItemDescription(rewardItemUIs[0].descriptableObject);
    }

    public void ShowItemDescription(IDescriptable descriptableObject)
    {
        if (descriptableObject == null)
            return;

        foreach(RewardWindowItemUI item in rewardItemUIs)
        {
            item.Select(item.descriptableObject == descriptableObject);
        }

        currentSelected = descriptableObject;

        descriptionText.UpdateUI(descriptableObject);
    }

    public void Select()
    {
        gameObject.SetActive(false);
        RewardGainedUI.Instance.Setup(currentSelected);
        
        if(currentSelected is EquipmentSO equipment)
        {
            equipmentInventory.TryAddEquipment(equipment);
        }
    }

    
}

public interface IDescriptable
{
    public string Title { get; }

    public string MainDescription { get; }

    public Sprite Sprite { get; }

    public List<SubDescriptionUIAndParams> SubDescriptionPrefabsAndParams { get; }
}


public abstract class SubDescriptionUI : MonoBehaviour
{
    public abstract void UpdateUI(params object[] obj);
}

public class SubDescriptionUIAndParams
{
    public SubDescriptionUI UI;
    public object[] Params;

    public SubDescriptionUIAndParams(SubDescriptionUI UI, object[] Params)
    {
        this.UI = UI;
        this.Params = Params;
    }
}
