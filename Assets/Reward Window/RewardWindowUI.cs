using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
public class RewardWindowUI : MonoBehaviour
{
    [SerializeField] private RewardWindowItemUI rewardItemUIPrefab;

    [SerializeField] private readonly List<RewardWindowItemUI> rewardItemUIs = new List<RewardWindowItemUI>();

    [SerializeField] private RectTransform itemHolder;

    [SerializeField] private DescriptionUI descriptionText;

    public void Setup(List<IDescriptable> descriptables)
    {
        rewardItemUIs.DestroyAndClearList();

        foreach (IDescriptable descriptable in descriptables)
        {
            RewardWindowItemUI rewardWindowItemUI = Instantiate(rewardItemUIPrefab, itemHolder);
            rewardWindowItemUI.Setup(descriptable);
            rewardItemUIs.Add(rewardWindowItemUI);
        }
    }

    public void ShowItemDescription(RewardWindowItemUI rewardWindowItemUI)
    {
        if (rewardWindowItemUI == null)
            return;

        descriptionText.UpdateUI(rewardWindowItemUI.descriptableObject);
    }

    public void Select()
    {
       
    }
}

public class RewardWindowItemUI : MonoBehaviour
{
    [SerializeField]
    private Image objectImage;
    [SerializeField]
    private float spriteSize = 40f;


    public IDescriptable descriptableObject;

    public void Setup(IDescriptable descriptableObject)
    {
        this.descriptableObject = descriptableObject;
        objectImage.sprite = descriptableObject.Sprite;
        objectImage.NativeRatioScaleToSize(spriteSize);
    }
}

public interface IDescriptable
{
    public string Title { get; }

    public string MainDescription { get; }

    public Sprite Sprite { get; }

    public Dictionary<SubDescriptionUI, object[]> SubDescriptionPrefabsAndParams { get; }
}


public abstract class SubDescriptionUI : MonoBehaviour
{
    public abstract void UpdateUI(params object[] obj);

    public abstract float Height { get; }
}
