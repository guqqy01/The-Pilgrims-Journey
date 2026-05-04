using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InputQuery : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI queryText;
    [SerializeField]
    private TextMeshProUGUI yesText;
    [SerializeField]
    private TextMeshProUGUI noText;

    private Action<bool> action;

    public void Setup(string queryString, Action<bool> action, string yesString = "yes", string noString = "no")
    {
        queryText.text = queryString;
        yesText.text = yesString;
        noText.text = noString;
        this.action = action;
    }



    public void ButtonPress(bool value)
    {
        action(value);
        gameObject.SetActive(false);
    }
}
