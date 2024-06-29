using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InfoDisplay : MonoBehaviour
{
    [SerializeField]
    private Text title;
    [SerializeField]
    private Text description;
    [SerializeField]
    private Text goldAmount;

    private void Awake()
    {
        IInfo.onInfoSelected += UpdateUI;
    }

    private void UpdateUI(IInfo selectedInfo)
    {
        if (title != null)
            title.text = selectedInfo.Title;
        if (description != null)
            description.text = selectedInfo.Description;
        if(goldAmount != null)
            goldAmount.text = selectedInfo.GoldAmount;
    }
}

public interface IInfo
{
    public string Title { get; }
    public string Description { get; }
    public string GoldAmount { get; }

    public delegate void OnInfoSelected(IInfo info);
    public static OnInfoSelected onInfoSelected { get; set; }
}