#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class TextHelper
{
    [MenuItem("Tools/Update All Text To TMPro (All Scenes & Prefabs)")]
    private static void GetFontThenUpdateText()
    {
        TMPFontAssetPicker.ShowWindow((font) =>
        {
            if (font == null)
            {
                Debug.LogWarning("No font selected. Aborting conversion.");
                return;
            }

            ConvertAllScenesAndPrefabs(font);
        });
    }

    private static void ConvertAllScenesAndPrefabs(TMP_FontAsset font)
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        if (sceneGuids.Length == 0) Debug.Log("No scenes found in project.");

        List<Scene> openedScenes = new List<Scene>();
        Scene originalActive = SceneManager.GetActiveScene();

        try
        {
            foreach (var guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;

                bool pathIsReadOnly = false;
                try
                {
                    var fileInfo = new FileInfo(path);
                    pathIsReadOnly = fileInfo.Exists && fileInfo.IsReadOnly;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not inspect file attributes for scene {path}: {e.Message}");
                }


                Scene scene;
                try
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to open scene {path}: {e.Message}");
                    continue;
                }

                openedScenes.Add(scene);

                bool changed = ConvertTextInScene(scene, font);

                // If scene file was read-only we don't attempt to save (avoid exceptions or VCS conflicts)
                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);

                    if (!pathIsReadOnly)
                    {
                        try
                        {
                            EditorSceneManager.SaveScene(scene);
                            Debug.Log($"Saved changes to scene: {path}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Failed to save scene {path}: {e.Message}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Scene changed but is read-only, not saved: {path}");
                    }
                }
                else
                {
                    Debug.Log($"No Text components found in scene: {path}");
                }
            }
        }
        finally
        {
            foreach (var scene in openedScenes)
            {
                if (scene.isLoaded && scene != originalActive)
                    EditorSceneManager.CloseScene(scene, true);
            }

            if (originalActive.IsValid())
                EditorSceneManager.OpenScene(originalActive.path, OpenSceneMode.Single);
        }

        // Prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        if (prefabGuids.Length == 0) Debug.Log("No prefabs found in project.");

        int prefabCount = 0;
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;

            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabRoot == null) continue;

            // Work on an instantiated copy so we can modify safely
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
            if (instance == null) continue;

            try
            {
                bool changed = ConvertTextInGameObjectHierarchy(instance, font, createUndo: false, logName: path);

                if (changed)
                {
                    // Apply modifications back to the prefab asset
                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                    prefabCount++;
                    Debug.Log($"Updated prefab: {path}");
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        Debug.Log($"Prefab conversion complete. Prefabs changed: {prefabCount}");
        AssetDatabase.Refresh();

    }

    private static bool ConvertTextInScene(Scene scene, TMP_FontAsset font)
    {
        bool anyChanged = false;

        if (!scene.IsValid()) return false;

        GameObject[] roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            bool changedThisRoot = ConvertTextInGameObjectHierarchy(root, font, createUndo: true, logName: root.name);
            anyChanged |= changedThisRoot;
        }

        return anyChanged;
    }

    private static bool ConvertTextInGameObjectHierarchy(GameObject root, TMP_FontAsset font, bool createUndo, string logName)
    {
        Text[] texts = root.GetComponentsInChildren<Text>(true);
        if (texts == null || texts.Length == 0) return false;

        foreach (var text in texts)
        {
            if (text == null) continue;

            GameObject go = text.gameObject;
            string originalName = go.name;

            if (createUndo)
            {
                // Register undo for the GameObject so changes can be undone in the scene
                Undo.RegisterCompleteObjectUndo(go, "Replace Text with TMP");
            }

            // Cache properties
            string content = text.text;
            Color color = text.color;
            bool raycast = text.raycastTarget;
            TextAnchor anchor = text.alignment;

            // Destroy the legacy component and add TMP
            if (createUndo)
            {
                Undo.DestroyObjectImmediate(text);
            }
            else
            {
                Object.DestroyImmediate(text, true);
            }

            TextMeshProUGUI tmp = null;
            if (createUndo)
            {
                tmp = Undo.AddComponent<TextMeshProUGUI>(go);
            }
            else
            {
                tmp = go.AddComponent<TextMeshProUGUI>();
            }

            // Basic font size heuristic: if localScale is non-1, try to keep visual size
            float fontSize = Mathf.RoundToInt(go.transform.localScale.x * 10.666f);
            go.transform.localScale = Vector3.one;
            tmp.fontSize = Mathf.Max(1, fontSize);
            tmp.color = color;
            tmp.raycastTarget = raycast;
            tmp.font = font;
            tmp.text = content;
            tmp.alignment = MapTextAnchorToTMP(anchor);

            Debug.Log($"Replaced Text on {logName} / {originalName}");
        }

        return true;
    }

    private static TextAlignmentOptions MapTextAnchorToTMP(TextAnchor anchor)
    {
        // Rough mapping between UnityEngine.TextAnchor and TMPro.TextAlignmentOptions
        switch (anchor)
        {
            case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
            default: return TextAlignmentOptions.TopLeft;
        }
    }
}

public class TMPFontAssetPicker : EditorWindow
{
    private TMP_FontAsset selectedFont;
    private Action<TMP_FontAsset> onFontSelected;

    public static void ShowWindow(Action<TMP_FontAsset> onSelected)
    {
        TMPFontAssetPicker window = CreateInstance<TMPFontAssetPicker>();
        window.titleContent = new GUIContent("Select TMP Font Asset");
        window.onFontSelected = onSelected;
        window.minSize = new Vector2(360, 80);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Pick a TextMeshPro Font Asset:", EditorStyles.boldLabel);
        selectedFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", selectedFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(8);

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

#endif