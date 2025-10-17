using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Map
{
    [CustomEditor(typeof(GlobalMapGenerator))]
    public class GlobalMapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GlobalMapGenerator script = (GlobalMapGenerator)target;

            base.OnInspectorGUI();

            if (GUILayout.Button("GenerateMap"))
            {
                script.GenerateMap(Random.Range(int.MinValue, int.MaxValue));
                SceneView.RepaintAll();

                if(!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}
