using UnityEngine;
using TMPro;

public class BuffSubDescriptionUI : SubDescriptionUI
{
    [SerializeField]
    private TextMeshProUGUI buffText;

    public override void UpdateUI(ISubDescriptionData subData)
    {
        if (subData is not BuffSubDescriptionData buffData) return;
        buffText.text = buffData.StatString + "\r\n" + buffData.Buff.GetDescription();
    }
}
