using UnityEngine;
using UnityEditor;
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
                script.CreatePath(script.start, script.startDirection, script.targetDirections);
                SceneView.RepaintAll();
            }
        }
    }
}


