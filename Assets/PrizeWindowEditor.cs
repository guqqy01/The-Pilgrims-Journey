using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrizeWindow))]
public class PrizeWindowEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Load Prizes"))
        {
            ((PrizeWindow)target).LoadPrize(3);
        }
    }
}