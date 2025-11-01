using UnityEngine;
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
