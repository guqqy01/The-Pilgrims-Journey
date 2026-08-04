using UnityEngine;
using UnityEditor;
using GlobalMap;
using UnityEngine.UIElements;

namespace GlobalMap
{
    [CustomEditor(typeof(GlobalMapGenerationSettings))]
    public class GlobalMapGenerationSettingsEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Draw all the serializable fields first
            root.Add(new IMGUIContainer(() => DrawDefaultInspector()));

            return root;
        }
    }
}


