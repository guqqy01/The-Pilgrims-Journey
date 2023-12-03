using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(GlobalMapGenerator))]
public class GlobalMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GlobalMapGenerator script = (GlobalMapGenerator)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("GenerateMap"))
        {
            script.GenerateMap();
            SceneView.RepaintAll();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}