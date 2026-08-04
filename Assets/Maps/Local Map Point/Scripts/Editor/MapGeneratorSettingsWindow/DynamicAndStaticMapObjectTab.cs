using UnityEngine;
using UnityEditor;

public class DynamicAndStaticMapObjectTab : EditorMenuToolbarWindow, IEditorMenuToolbarTab
{
    public string Name => "Map Objects";

    public DynamicAndStaticMapObjectTab(SerializedObject serializedObject)
    {
        _tabs.Clear();

        StaticMapObjectsTab staticMapObjects = new StaticMapObjectsTab(serializedObject);
        DynamicMapObjectsTab dynamicMapObjects = new DynamicMapObjectsTab(serializedObject);

        _tabs.Add(staticMapObjects);
        _tabs.Add(dynamicMapObjects);
    }

    public void DrawTab(Rect rect)
    {
        DrawToolbarWindow(rect);
    }

    public float GetHeight(Rect rect)
    {
        return rect.height;
    }
}