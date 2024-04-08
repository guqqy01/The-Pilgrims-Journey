using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ResourceUI : MonoBehaviour
{
    [SerializeField]
    private Text goldText;

    [SerializeField]
    private Text diamondText;

    private void Start()
    {
        ResourceManager.Instance.onGoldChange += OnGoldChange;
    }

    private void OnGoldChange(int currentGold)
    {
        goldText.text = currentGold.ToString();
    }

}
