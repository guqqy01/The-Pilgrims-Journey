using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RewardItemUI : MonoBehaviour
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private Text text;

    public void Init(Sprite sprite, string number)
    {
        image.sprite = sprite;
        text.text = "x" + number.ToString();
        image.SetNativeSize();
    }
}
