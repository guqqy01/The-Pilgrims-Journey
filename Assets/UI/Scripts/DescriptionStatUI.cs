using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionStatUI : MonoBehaviour
{
    [SerializeField]
    private Text buffText;

    public void Setup(Buff buff, UnitStatsEnum Stat)
    {
        string baseBuffPositiveOrNegative = buff.BaseBuff > 0 ? "+" : "";
        string multiplierBuffPositiveOrNegative = buff.Multiplier > 0 ? "+" : "";
        string additionalBuffPositiveOrNegative = buff.AdditionalBuff > 0 ? "+" : "";
        string BaseBuffString = buff.BaseBuff != 0 ? $"\r\nBase:{baseBuffPositiveOrNegative}{buff.BaseBuff.Value}" : "";
        string MultiplierBuffString = buff.Multiplier != 0 ? $"\r\nMultiplier: {buff.Multiplier}%" : "";
        string AdditionalBuffString = buff.AdditionalBuff != 0 ? $"\r\nAdditional: {additionalBuffPositiveOrNegative}{buff.AdditionalBuff.Value}" : "";



        string totalBuffString = Stat.ToString().AddSpacesBeforeCapsRegex()
            + BaseBuffString
            + MultiplierBuffString
            + AdditionalBuffString;

        buffText.text = totalBuffString;
    }

    
}
