using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RewardItemUI : MonoBehaviour
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private TextMeshProUGUI text;

    public void Init(Sprite sprite, string number)
    {
        image.sprite = sprite;
        text.text = "x" + number.ToString();
        image.SetNativeSize();
    }
}
