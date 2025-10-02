using UnityEditor;
using UnityEngine;
using TMPro;
using System.Reflection;

public class FontSOChecker
{
    [MenuItem("Tools/Check Missing Fonts in ScriptableObjects")]
    public static void CheckFontsInScriptableObjects()
    {
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so == null)
                continue;

            var fields = so.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(Font))
                {
                    Font font = field.GetValue(so) as Font;
                    if (font == null)
                        Debug.LogWarning($"Missing Font in '{so.name}' ({so.GetType().Name}) at '{path}'", so);
                }
#if TMP_PRESENT
                else if (field.FieldType == typeof(TMP_FontAsset))
                {
                    TMP_FontAsset tmpFont = field.GetValue(so) as TMP_FontAsset;
                    if (tmpFont == null)
                        Debug.LogWarning($"Missing TMP_FontAsset in '{so.name}' ({so.GetType().Name}) at '{path}'", so);
                }
#endif
            }
        }
        Debug.Log("FontSOChecker: Scan complete.");
    }
}