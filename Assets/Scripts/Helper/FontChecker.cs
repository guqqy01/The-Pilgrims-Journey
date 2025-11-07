using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FontChecker
{
    [MenuItem("Tools/Check Missing Fonts")]
    public static void CheckFonts()
    {
        var texts = GameObject.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int missingFonts = 0;
        foreach (var text in texts)
        {
            if (text.font == null)
            {
                Debug.LogWarning($"Missing font on {text.gameObject.name}", text.gameObject);
                missingFonts++;
            }
        }
        Debug.Log($"There are {missingFonts} fonts missing out of {texts.Length}");
    }
}