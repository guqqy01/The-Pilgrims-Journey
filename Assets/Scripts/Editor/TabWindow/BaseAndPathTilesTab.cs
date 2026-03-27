using UnityEngine;
using UnityEditor;

public class BaseAndPathTilesTab : EditorMenuToolbarWindow, IEditorMenuToolbarTab
{
    public string Name => "Base Tiles";

    public BaseAndPathTilesTab(SerializedObject serializedObject)
    {
        Tabs.Clear();

        TileTab mapTilesTab = new TileTab("Map Tiles", serializedObject, "baseTiles", "Map Tiles");
        TileTab pathTilesTab = new TileTab("Path Tiles", serializedObject, "pathTiles", "Path Tiles");

        Tabs.Add(mapTilesTab);
        Tabs.Add(pathTilesTab);
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
