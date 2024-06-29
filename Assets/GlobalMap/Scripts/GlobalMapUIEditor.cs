using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Map
{
    [CustomEditor(typeof(GlobalMapUI))]
    public class GlobalMapUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GlobalMapUI script = (GlobalMapUI)target;

            base.OnInspectorGUI();
            if (Application.isPlaying)
                if (GUILayout.Button("Update UI"))
                {
                    script.UpdateUI();
                    SceneView.RepaintAll();
                }
        }
    }
}
