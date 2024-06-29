using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LayoutContentSizerBaseParent))]
public class LayoutContentSizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button(new GUIContent("Update Layout")))
        {
            Undo.RecordObject(target, "Update Layout");
            ((LayoutContentSizerBaseParent)target).UpdateAllLayoutsDeepestFirst();
        }
    }
}