using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;
public static class TextHelper
{

    [MenuItem("Tools/Update All Text To TMPro")]
    private static void GetFontThenUpdateText()
    {
        TMPFontAssetPicker.ShowWindow((font) =>
        {
            // This is your continuation logic after the user picks a font
            Debug.Log("User picked font: " + font.name);

            // Example: apply it somewhere
            UpdateText(font);
        });
    }

    private static void UpdateText(TMP_FontAsset font)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene == null)
        {
            Debug.Log("active scene is null"); 
            return;
        }

        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        if(rootObjects == null)
        {
            Debug.Log("rootObjects is null");
            return;
        }

        if(rootObjects.Length == 0)
        {
            Debug.Log("rootObjects is empty");
            return;
        }

        

        foreach(GameObject obj in rootObjects)
        {
            Text[] texts = obj.GetComponentsInChildren<Text>(true);

            foreach(Text text in texts)
            {
                GameObject go = text.gameObject;
                string str = text.text;


                Undo.RegisterCompleteObjectUndo(go, "Replace Text with TMP");
                Debug.Log($"Removed text on {go.name}");
                Undo.DestroyObjectImmediate(text);
                Debug.Log($"TMPro added to {go.name}");
                var textMeshPro = Undo.AddComponent<TextMeshProUGUI>(go);

                textMeshPro.alignment = (TextAlignmentOptions)(int)text.alignment; // rough mapping
                textMeshPro.color = text.color;
                textMeshPro.raycastTarget = text.raycastTarget;
                textMeshPro.font = font;
                textMeshPro.text = str;
                go.transform.localScale = Vector3.one;
            }
        }

        EditorSceneManager.MarkSceneDirty(activeScene);
    }
}


public class TMPFontAssetPicker : EditorWindow
{
    private TMP_FontAsset selectedFont;
    private System.Action<TMP_FontAsset> onFontSelected;

    /// <summary>
    /// Opens the picker window and continues with the provided callback once a font is chosen.
    /// </summary>
    public static void ShowWindow(System.Action<TMP_FontAsset> onSelected)
    {
        TMPFontAssetPicker window = CreateInstance<TMPFontAssetPicker>();
        window.titleContent = new GUIContent("Select TMP Font Asset");
        window.onFontSelected = onSelected;
        window.ShowUtility(); // Utility window = small popup
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Pick a TextMeshPro Font Asset:", EditorStyles.boldLabel);
        selectedFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", selectedFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(10);

        using (new EditorGUI.DisabledScope(selectedFont == null))
        {
            if (GUILayout.Button("Continue"))
            {
                onFontSelected?.Invoke(selectedFont);
                Close();
            }
        }

        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }
}