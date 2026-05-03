using UnityEngine;
using TMPro;

public class BuffSubDescriptionUI : SubDescriptionUI
{
    [SerializeField]
    private TextMeshProUGUI buffText;

    public void Setup(Buff buff, string statString)
    {
        string totalBuffString = statString + "\r\n" + buff.GetDescription();

        buffText.text = totalBuffString;
    }

    public override void UpdateUI(params object[] obj)
    {
        if (obj.Length < 2)
            return;

        if (obj[0] is Buff buff && obj[1] is string statString)
        {
            Setup(buff, statString);
        }
    }
}
