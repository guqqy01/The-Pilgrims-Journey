using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FontChecker
{
    [MenuItem("Tools/Check Missing Fonts")]
    public static void CheckFonts()
    {
        var texts = GameObject.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var text in texts)
        {
            if (text.font == null)
                Debug.LogWarning($"Missing font on {text.gameObject.name}", text.gameObject);
        }
    }
}