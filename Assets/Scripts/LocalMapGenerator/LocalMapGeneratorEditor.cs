using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Map
{
    [CustomEditor(typeof(LocalMapGenerator))]
    public class LocalMapGeneratorEditor : Editor 
    {
        public override void OnInspectorGUI()
        {
            LocalMapGenerator script = (LocalMapGenerator)target;

            base.OnInspectorGUI();

            if (GUILayout.Button("Generate Path"))
            {
                script.CreateMap();
                SceneView.RepaintAll();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}


