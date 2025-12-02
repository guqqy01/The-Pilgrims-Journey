using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Collections;
using Unity.VisualScripting;

public class DescriptionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI mainDescriptionText;

    [SerializeField] private readonly List<SubDescriptionUI> subDescriptionsUIs = new List<SubDescriptionUI>();

    [SerializeField] private Transform subDescriptionHolder;

    [SerializeField] private LayoutContentSizerBaseParent layoutContentSizer;

    [SerializeField]
    private RectTransform contentTransform;
    [SerializeField]
    private RectTransform viewportTransform;

    [SerializeField]
    private Scrollbar scrollbar;

    private void Awake()
    {
        Info.onInfoSelected += UpdateUI;
    }

    public void UpdateUI(IDescriptable descriptable)
    {
        titleText.text = descriptable.Title;
        mainDescriptionText.text = descriptable.MainDescription;

        subDescriptionsUIs.DestroyAndClearList();

        foreach (var kvp in descriptable.SubDescriptionPrefabsAndParams)
        {
            SubDescriptionUI prefab = kvp.UI;
            object[] paramObjects = kvp.Params;

            SubDescriptionUI newSubDescription = Instantiate(prefab, subDescriptionHolder);
            newSubDescription.UpdateUI(paramObjects);

            subDescriptionsUIs.Add(newSubDescription);
        }

        
        layoutContentSizer.WaitOneFrameThenUpdateLayout();


        if(gameObject.activeInHierarchy)
            StartCoroutine(WaitTwoFramesThenSetHeight());
    }

    private IEnumerator WaitTwoFramesThenSetHeight()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        

        scrollbar.size = Mathf.Min(viewportTransform.sizeDelta.y / contentTransform.sizeDelta.y, 1);
        contentTransform.anchoredPosition = new Vector2(0, -contentTransform.sizeDelta.y / 2f);
    }
}
