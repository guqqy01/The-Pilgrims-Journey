using UnityEngine;
using UnityEditor;

public class MapObjectsTab : IEditorMenuToolbarTab
{
    public string Name => "Map Objects";

    private ReordableFoldoutList _overlayMapObjectList;

    public MapObjectsTab(SerializedObject serializedObject)
    {
        SerializedProperty mapObjectsProp = serializedObject.FindProperty("mapObjects");
        _overlayMapObjectList = new ReordableFoldoutList(serializedObject, mapObjectsProp, (index) => 128f, DrawOverlayMapObject, "Map Objects");
    }

    public void DrawTab(Rect rect)
    {
        _overlayMapObjectList.DoList(rect);
    }

    public float GetHeight(Rect rect)
    {
        return _overlayMapObjectList.Height;
    }

    private Texture2D GetOverlayPreview(GameObject go)
    {
        if (go == null) return null;
        Texture2D tex = AssetPreview.GetAssetPreview(go);
        if (tex == null)
            tex = AssetPreview.GetMiniThumbnail(go);
        return tex;
    }

    private void DrawOverlayMapObject(Rect rect, SerializedProperty element, int index)
    {
        rect.y += 2;
        rect.height -= 4;

        SerializedProperty nameProp = element.FindPropertyRelative("name");
        SerializedProperty overlayProp = element.FindPropertyRelative("overlay");
        SerializedProperty sizeProp = element.FindPropertyRelative("size");
        SerializedProperty curveProp = element.FindPropertyRelative("amountToSpawnCurve");

        float previewSize = rect.height - EditorGUIUtility.singleLineHeight;

        // --- Left: preview ---
        Rect previewRect = new Rect(rect.x, rect.y, previewSize, previewSize);


        Texture2D previewTex = null;
        if (overlayProp != null && overlayProp.objectReferenceValue != null)
        {
            GameObject go = overlayProp.objectReferenceValue as GameObject;
            if (go != null)
            {
                previewTex = GetOverlayPreview(go);
            }
        }

        if (previewTex != null)
            GUI.Label(previewRect, previewTex);

        // --- Under preview: ObjectField for overlay ---
        Rect overlayFieldRect = new Rect(rect.x, rect.yMax - EditorGUIUtility.singleLineHeight, previewSize, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(overlayFieldRect, overlayProp, GUIContent.none);

        // --- Right side: other fields ---
        float rightX = previewRect.xMax + 6;
        float rightWidth = rect.width - previewSize - 6;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2;

        Rect nameRect = new Rect(rightX, rect.y, rightWidth, lineHeight);
        Rect sizeRect = new Rect(rightX, nameRect.yMax + spacing, rightWidth, lineHeight * 2);
        Rect curveRect = new Rect(rightX, sizeRect.yMax + spacing, rightWidth, lineHeight);

        EditorGUI.PropertyField(nameRect, nameProp);
        EditorGUI.PropertyField(sizeRect, sizeProp);
        EditorGUI.PropertyField(curveRect, curveProp);
    }

}