using NUnit.Framework.Interfaces;
using Relics;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;


[InitializeOnLoad]
public static class RelicSOProjectIconDrawer
{
    static readonly Type targetType = typeof(RelicSO);
    static readonly Dictionary<string, Texture2D> s_cache = new Dictionary<string, Texture2D>();

    static RelicSOProjectIconDrawer()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;

        AssemblyReloadEvents.afterAssemblyReload += ClearCache;
        EditorApplication.playModeStateChanged += _ => ClearCache();
    }

    static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(assetPath)) return;

        UnityEngine.Object main = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (main is not RelicSO relic) return;

        if (AssetPreview.IsLoadingAssetPreviews())
        {
            EditorApplication.delayCall += EditorApplication.RepaintProjectWindow;
            return;
        }

        if (!s_cache.TryGetValue(assetPath, out var tex) || tex == null)
        {
            tex = GetPreview(relic);
            if (tex != null)
            {
                s_cache[assetPath] = tex;
                EditorApplication.RepaintProjectWindow();
            }
            else
            {
                EditorApplication.delayCall += EditorApplication.RepaintProjectWindow;
                return;
            }
        }


        if (tex == null) return;

        DrawIcon(selectionRect, tex, main);
    }

    static void DrawIcon(Rect selectionRect, Texture2D tex, UnityEngine.Object main)
    {
        //selectionRect.height -= EditorGUIUtility.singleLineHeight;

        bool isListMode = selectionRect.height <= 20f;

        if (isListMode)
        {
            // Draw a small icon next to the asset name
            float size = selectionRect.height - 2;
            Rect iconRect = new Rect(selectionRect)
            {
                xMin = selectionRect.xMin + 5f,
                width = size,
                yMin = selectionRect.yMin + 1f,
                height = size,
            };

            if (iconRect.x < 0) // avoid drawing offscreen
                iconRect.x = 2;

            bool isSelected = Selection.activeObject == main;
            Color bg;
            if (isSelected)
            {
                bg = new Color(0.23921568627f, 0.37647058823f, 0.56862745098f);
            }
            else
            {
                // Draw background based on Editor skin
                bg = EditorGUIUtility.isProSkin
                    ? new Color(0.2f, 0.2f, 0.2f)
                    : new Color(0.85f, 0.85f, 0.85f);
            }

            EditorGUI.DrawRect(iconRect, bg);
            GUI.DrawTexture(iconRect, tex, ScaleMode.ScaleToFit, true);
        }
        else
        {
            selectionRect.yMax -= 15f;

            // Draw background based on Editor skin
            Color bg = EditorGUIUtility.isProSkin
                ? new Color(0.2f, 0.2f, 0.2f)
                : new Color(0.85f, 0.85f, 0.85f);

            EditorGUI.DrawRect(selectionRect, bg);

            // Shrink the rect slightly to fit nicely
            selectionRect = new Rect(
                selectionRect.x + 2,
                selectionRect.y + 2,
                selectionRect.height - 4,
                selectionRect.height - 4
            );

            GUI.DrawTexture(selectionRect, tex, ScaleMode.ScaleToFit, true);
        }
    }

    [InitializeOnLoadMethod]
    static void InitRepaintLoop()
    {
        EditorApplication.update += () =>
        {
            if (AssetPreview.IsLoadingAssetPreviews())
                EditorApplication.RepaintProjectWindow();
        };
    }


    static Texture2D GetPreview(RelicSO relic)
    {
        if (relic.sprite == null) return null;

        // First try Unity's preview system
        var tex = AssetPreview.GetAssetPreview(relic.sprite);
        if (tex != null && tex != Texture2D.whiteTexture) // crude check against placeholder
            return tex;

        if (AssetPreview.IsLoadingAssetPreviews())
            return null;

        // Fallback: manually crop the sprite from its source texture
        return ExtractSpriteTexture(relic.sprite);
    }

    static Texture2D ExtractSpriteTexture(Sprite sprite)
    {
        var src = sprite.texture;
        if (src == null) return null;

        Rect r = sprite.textureRect;
        int x = Mathf.RoundToInt(r.x);
        int y = Mathf.RoundToInt(r.y);
        int w = Mathf.RoundToInt(r.width);
        int h = Mathf.RoundToInt(r.height);

        RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0);

        Graphics.Blit(src, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.ReadPixels(new Rect(x, y, w, h), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return tex;
    }

    static void ClearCache()
    {
        foreach (var tex in s_cache.Values)
            if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
        s_cache.Clear();
    }

    public static void MarkAssetDirty(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return;
        if (s_cache.TryGetValue(assetPath, out var tex) && tex != null)
            UnityEngine.Object.DestroyImmediate(tex);
        s_cache.Remove(assetPath);

        EditorApplication.delayCall += EditorApplication.RepaintProjectWindow;
    }
}
