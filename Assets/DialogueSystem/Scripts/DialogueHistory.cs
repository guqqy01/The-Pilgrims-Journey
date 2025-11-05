using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
public class DialogueHistory : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;

    public List<string> history = new List<string>();

    public void AddHistory(string text)
    {
        history.Add(text);
        UpdateText();
    }

    public void UpdateText()
    {
        text.text = "";

        for (int i = 0; i < history.Count; i++)
        {
            text.text += history[i];

            if(i !=  history.Count - 1)
            {
                text.text += "\r\n\r\n";
                text.text += "------------------\r\n\r\n";
            }
        }
    }
}
