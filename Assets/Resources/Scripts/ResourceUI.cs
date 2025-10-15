using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ResourceUI : MonoBehaviour
{
    [SerializeField]
    private Text goldText;

    private void Start()
    {
        ResourceManager.Instance.onGoldChange += OnGoldChange;

        goldText.text = ResourceManager.Instance.Gold.ToString();
    }

    private void OnGoldChange(int currentGold)
    {
        goldText.text = currentGold.ToString();
    }

}
