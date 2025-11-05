using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DescriptionStatUI : SubDescriptionUI
{
    private RectTransform rectTranform;

    private void Awake()
    {
        rectTranform = GetComponent<RectTransform>();
    }

    [SerializeField]
    private TextMeshProUGUI buffText;

    public override float Height
    {
        get
        {
            if(rectTranform == null)
                rectTranform = GetComponent<RectTransform>();

            return rectTranform.sizeDelta.y;
        }
    }

    public void Setup(Buff buff, UnitStatsEnum Stat)
    {
        string totalBuffString = Stat.ToString().AddSpacesBeforeCapsRegex() + "\r\n" + buff.GetDescription();
        
        buffText.text = totalBuffString;
    }

    public override void UpdateUI(params object[] obj)
    {
        if(obj.Length < 2)
            return;

        Buff buff = (Buff)obj[0];
        UnitStatsEnum stat = (UnitStatsEnum)obj[1];

        Setup(buff, stat);
    }
}