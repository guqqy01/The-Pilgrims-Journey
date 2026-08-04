using UnityEngine;
using UnityEditor;
using LocalMap;

public class DynamicMapObjectsTab : IEditorMenuToolbarTab
{
    public string Name => "Dynamic";

    private ReorderableFoldoutList _overlayMapObjectList;

    public DynamicMapObjectsTab(SerializedObject serializedObject)
    {
        SerializedProperty dynamicMapObjectsProp = serializedObject.FindProperty(nameof(LocalMapGenerationSettings.dynamicMapObjects));
        _overlayMapObjectList = new ReorderableFoldoutList(serializedObject, dynamicMapObjectsProp, (index) => 128f, DrawOverlayMapObject, "Dynamic Map Objects");
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

        SerializedProperty prefabProp = element.FindPropertyRelative(nameof(DynamicMapObject.prefab));
        SerializedProperty nameProp = element.FindPropertyRelative(nameof(DynamicMapObject.name));
        SerializedProperty spawnRateProp = element.FindPropertyRelative(nameof(DynamicMapObject.spawnRate));
        SerializedProperty spawnRateRandomVariationMultipliereProp = element.FindPropertyRelative(nameof(DynamicMapObject.spawnRateRandomVariationMultiplier));
        SerializedProperty dynamicMapObjectSpawnMethodProp = element.FindPropertyRelative(nameof(DynamicMapObject.dynamicMapObjectSpawnMethod));

        float previewSize = rect.height - EditorGUIUtility.singleLineHeight;

        // --- Left: preview ---
        Rect previewRect = new Rect(rect.x, rect.y, previewSize, previewSize);


        Texture2D previewTex = null;
        if (prefabProp != null && prefabProp.objectReferenceValue != null)
        {
            GameObject go = prefabProp.objectReferenceValue as GameObject;
            if (go != null)
            {
                previewTex = GetOverlayPreview(go);
            }
        }

        if (previewTex != null)
            GUI.Label(previewRect, previewTex);

        // --- Under preview: ObjectField for overlay ---
        Rect overlayFieldRect = new Rect(rect.x, rect.yMax - EditorGUIUtility.singleLineHeight, previewSize, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(overlayFieldRect, prefabProp, GUIContent.none);

        // --- Right side: other fields ---
        float rightX = previewRect.xMax + 6;
        float rightWidth = rect.width - previewSize - 6;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2;

        Rect nameRect = new Rect(rightX, rect.y, rightWidth, lineHeight);
        Rect spawnRateRect = new Rect(rightX, nameRect.yMax + spacing, rightWidth, lineHeight);
        Rect spawnRateRandomVariationMultiplierRect = new Rect(rightX, spawnRateRect.yMax + spacing, rightWidth, lineHeight);
        Rect dynamicMapObjectSpawnMethodRect = new Rect(rightX, spawnRateRandomVariationMultiplierRect.yMax + spacing, rightWidth, lineHeight);

        EditorGUI.PropertyField(nameRect, nameProp);
        EditorGUI.PropertyField(spawnRateRect, spawnRateProp);
        EditorGUI.PropertyField(spawnRateRandomVariationMultiplierRect, spawnRateRandomVariationMultipliereProp);
        EditorGUI.PropertyField(dynamicMapObjectSpawnMethodRect, dynamicMapObjectSpawnMethodProp);
    }

}