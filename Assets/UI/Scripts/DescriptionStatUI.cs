using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionStatUI : SubDescriptionUI
{
    [SerializeField]
    private Text buffText;

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