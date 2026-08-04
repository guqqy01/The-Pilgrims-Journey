using UnityEngine;
using UnityEditor;

public class BaseAndPathTilesTab : EditorMenuToolbarWindow, IEditorMenuToolbarTab
{
    public string Name => "Base Tiles";

    public BaseAndPathTilesTab(SerializedObject serializedObject)
    {
        _tabs.Clear();

        TileTab mapTilesTab = new TileTab("Map Tiles", serializedObject, "baseTilesAndType", "Map Tiles");
        PathSettingsTileTab pathTilesTab = new PathSettingsTileTab("Path Tiles", serializedObject, "pathTilesAndType", "Path Tiles");

        _tabs.Add(mapTilesTab);
        _tabs.Add(pathTilesTab);
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
