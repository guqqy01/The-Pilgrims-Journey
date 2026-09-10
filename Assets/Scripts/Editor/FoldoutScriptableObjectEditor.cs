using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableObject), true)]
[CanEditMultipleObjects]
public class FoldoutScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (FoldoutGroupDrawer.HasFoldoutFields(target))
            FoldoutGroupDrawer.DrawInspectorWithFoldouts(serializedObject);
        else
            base.OnInspectorGUI();
    }
}