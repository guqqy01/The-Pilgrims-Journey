using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Equipment;
using Relics;
using Random = UnityEngine.Random;
public class RewardWindowUI : MonoBehaviour
{
    [SerializeField] private RewardWindowItemUI rewardItemUIPrefab;

    [SerializeField] private readonly List<RewardWindowItemUI> rewardItemUIs = new List<RewardWindowItemUI>();

    [SerializeField] private RectTransform itemHolder;

    [SerializeField] private DescriptionUI descriptionText;

    [SerializeField] private List<EquipmentSO> testEquipment;
    [SerializeField] private List<RelicSO> testRelics;

    private IDescriptable currentSelected;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestSetup();
        }
    }

    private void TestSetup()
    {
        int rand = Random.Range(0, 2);

        if(rand == 0)
        {
            List<IDescriptable> equipment = testEquipment.GetRandomAmount(3).Cast<IDescriptable>().ToList();

            Setup(equipment);
        }
        else
        {
            List<IDescriptable> relics = testRelics.GetRandomAmount(3).Cast<IDescriptable>().ToList();

            Setup(relics);
        }
    }

    public void Setup(List<IDescriptable> descriptables)
    {
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
    }

    public void ShowItemDescription(IDescriptable descriptableObject)
    {
        if (descriptableObject == null)
            return;

        currentSelected = descriptableObject;

        descriptionText.UpdateUI(descriptableObject);
    }

    public void Select()
    {
        gameObject.SetActive(false);
        RewardGainedUI.Instance.Setup(currentSelected);
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
