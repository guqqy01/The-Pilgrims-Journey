using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionStatUI : MonoBehaviour
{
    [SerializeField]
    private Text buffText;

    public void Setup(Buff buff, UnitStatsEnum Stat)
    {
        string totalBuffString = Stat.ToString().AddSpacesBeforeCapsRegex() + "\r\n" + buff.GetDescription();
        
        buffText.text = totalBuffString;
    }

    
}
