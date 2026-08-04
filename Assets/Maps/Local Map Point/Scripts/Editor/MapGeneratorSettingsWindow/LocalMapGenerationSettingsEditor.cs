using UnityEngine;
using UnityEditor;

namespace LocalMap.Editor
{
    [CustomEditor(typeof(LocalMapGenerationSettings))]
    public class LocalMapGenerationSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            LocalMapGenerationSettings settings = (LocalMapGenerationSettings)target;


            if(GUILayout.Button("Open Editor"))
            {
                LocalMapGeneratorSettingsEditorWindow.Init(settings);
            }
        }
    }

}
