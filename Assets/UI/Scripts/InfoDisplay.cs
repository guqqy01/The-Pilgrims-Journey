using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InfoDisplay : MonoBehaviour
{
    [SerializeField]
    protected TextMeshProUGUI title;
    [SerializeField]
    protected TextMeshProUGUI description;
    [SerializeField]
    protected TextMeshProUGUI goldAmount;

    private void Awake()
    {
        Info.onInfoSelected += UpdateUI;
    }

    protected virtual void UpdateUI(IInfo selectedInfo)
    {
        if (title != null)
            title.text = selectedInfo.Title;
        if (description != null)
            description.text = selectedInfo.Description;
        if(goldAmount != null)
            goldAmount.text = selectedInfo.GoldAmount;
    }
}

public static class Info
{
    public static IInfo CurrentlySelected;

    public delegate void OnInfoSelected(IInfo info);
    public static OnInfoSelected onInfoSelected { get; set; }

    public static void Select(IInfo info)
    {
        CurrentlySelected?.Deselect();

        CurrentlySelected = info;

        CurrentlySelected.Select();

        onInfoSelected?.Invoke(CurrentlySelected);
    }
}

public interface IInfo
{
    public string Title { get; }
    public string Description { get; }
    public string GoldAmount { get; }

    public virtual void Select() { }
    public virtual void Deselect() { }
}