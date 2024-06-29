using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GlobalMapLineUI))]
public class GlobalMapLineEditor : Editor
{
    [Range(0f, 1f)]
    public float time;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GlobalMapLineUI line = (GlobalMapLineUI)target;

        time = EditorGUILayout.FloatField(time);

        if (GUILayout.Button("SetColor"))
        {
            line.SetColorGradient(time);
        }
    }
}