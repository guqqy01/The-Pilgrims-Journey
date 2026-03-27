using UnityEngine;
using UnityEditor;
using System;

public class TerrainNoiseParentTab : EditorMenuScrollableToolbarWindow, IEditorMenuToolbarTab
{
    public string Name => "Terrain Noise";
    public const string TerrainPropertyName = "terrainNoiseLists";

    public TerrainNoiseParentTab(SerializedObject serializedObject) : base(serializedObject, TerrainPropertyName)
    {
        Tabs.Clear();

        SerializedProperty TerrainNoiseProperty = serializedObject.FindProperty(TerrainPropertyName);

        for (int i = 0; i < TerrainNoiseProperty.arraySize; i++)
        {
            TerrainNoiseTab terrainNoiseTab = new TerrainNoiseTab(serializedObject, TerrainNoiseProperty.GetArrayElementAtIndex(i));
            Tabs.Add(terrainNoiseTab);
        }
    }


    public void DrawTab(Rect rect)
    {
        SerializedProperty property = _serializedObject.FindProperty(TerrainPropertyName);

        // Grow
        while (Tabs.Count < property.arraySize)
        {
            var tab = new TerrainNoiseTab(_serializedObject, property.GetArrayElementAtIndex(Tabs.Count));
            tab.OnRequestRepaint += () => OnTabChange?.Invoke();
            Tabs.Add(tab);
        }

        // Shrink
        while (Tabs.Count > property.arraySize)
        {
            if (Tabs[Tabs.Count - 1] is IDisposable disposable)
                disposable.Dispose();

            Tabs.RemoveAt(Tabs.Count - 1);
        }

        DrawToolbarWindow(rect);
    }



    public float GetHeight(Rect rect)
    {
        return rect.height;
    }
}
