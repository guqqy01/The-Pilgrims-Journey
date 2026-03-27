using UnityEngine;
using UnityEditor;

public class TileTab : IEditorMenuToolbarTab
{
    public TileTab(string name, SerializedObject serializedObject, string property, string header)
    {
        Name = name;

        SerializedProperty baseTilesProperty = serializedObject.FindProperty(property).FindPropertyRelative("weightedList");
        _tilesList = new ReordableFoldoutList(serializedObject, baseTilesProperty, (index) => 64f, DrawTile, header);
    }

    public string Name { get; set; }

    private ReordableFoldoutList _tilesList;

    public void DrawTab(Rect rect)
    {
        _tilesList.DoList(rect);
    }

    public float GetHeight(Rect rect)
    {
        return _tilesList.Height;
    }

    private void DrawTile(Rect rect, SerializedProperty baseTileProperty, int index)
    {
        float spriteLength = rect.height - 2;
        Rect spriteRect = new Rect(rect) { yMin = rect.yMin + 2, height = spriteLength, width = spriteLength };
        Rect spriteObjectPickerRect = new Rect(rect) { xMin = spriteRect.xMax, yMin = rect.yMin + 2, height = EditorGUIUtility.singleLineHeight };
        Rect weightRect = new Rect(rect) { xMin = spriteRect.xMax, yMin = spriteObjectPickerRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };

        SerializedProperty spriteProperty = baseTileProperty.FindPropertyRelative("item");
        SerializedProperty weightCurveProperty = baseTileProperty.FindPropertyRelative("weightCurve");

        Sprite sprite = spriteProperty.objectReferenceValue as Sprite;

        sprite.DrawSpriteInRect(spriteRect);

        EditorGUI.ObjectField(spriteObjectPickerRect, spriteProperty, new GUIContent("Tile"));

        EditorGUI.PropertyField(weightRect, weightCurveProperty);
    }
}
