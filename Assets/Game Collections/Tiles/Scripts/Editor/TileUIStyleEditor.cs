using System;
using System.Collections.Generic;
using Tiles;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileUIStyle))]
public class TileUIStyleEditor : Editor
{
    private static readonly (string name, Vector2Int[] cells)[] Shapes =
    {
        ("Single",  new[] { V(0,0) }),
        ("Domino",  new[] { V(0,0), V(1,0) }),
        ("Square 2x2", new[] { V(0,0), V(1,0), V(0,1), V(1,1) }),
        ("Line 3",  new[] { V(0,0), V(1,0), V(2,0) }),
        ("L",       new[] { V(0,0), V(0,1), V(0,2), V(1,0) }),
        ("T",       new[] { V(0,1), V(1,1), V(2,1), V(1,0) }),
        ("S / Z",   new[] { V(0,0), V(1,0), V(1,1), V(2,1) }),
        ("Plus",    new[] { V(1,0), V(0,1), V(1,1), V(2,1), V(1,2) }),
        ("U",       new[] { V(0,0), V(1,0), V(2,0), V(0,1), V(2,1) }),
        ("Ring 3x3", new[] { V(0,0), V(1,0), V(2,0), V(0,1), V(2,1), V(0,2), V(1,2), V(2,2) }),
        ("Block 3x3", new[] { V(0,0), V(1,0), V(2,0), V(0,1), V(1,1), V(2,1), V(0,2), V(1,2), V(2,2) }),
    };

    private static readonly string[] ShapeNames = BuildShapeNames();

    private static readonly Stamp.ClockwiseRotations[] Rotations =
    {
        Stamp.ClockwiseRotations.None,
        Stamp.ClockwiseRotations.Deg90,
        Stamp.ClockwiseRotations.Deg180,
        Stamp.ClockwiseRotations.Deg270,
    };
    private static readonly string[] RotationNames = { "0°", "90°", "180°", "270°" };

    private int _shapeIndex = 4; // L shows both edge types and an inner corner
    private int _rotationIndex;

    private Sprite _previewSprite;
    private int _previewKey;
    private string _previewError;

    private static Vector2Int V(int x, int y) => new Vector2Int(x, y);

    private static string[] BuildShapeNames()
    {
        var names = new string[Shapes.Length];
        for (int i = 0; i < Shapes.Length; i++) names[i] = Shapes[i].name;
        return names;
    }

    private void OnDisable() => ReleasePreview();

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
            ReleasePreview(); // force rebuild on next repaint
    }

    // ── Preview window ──────────────────────────────────────────────

    public override bool HasPreviewGUI() => true;

    public override GUIContent GetPreviewTitle() => new GUIContent("Tile Preview");

    public override void OnPreviewSettings()
    {
        EditorGUI.BeginChangeCheck();
        _shapeIndex = EditorGUILayout.Popup(_shapeIndex, ShapeNames,
            EditorStyles.toolbarPopup, GUILayout.Width(100));
        _rotationIndex = EditorGUILayout.Popup(_rotationIndex, RotationNames,
            EditorStyles.toolbarPopup, GUILayout.Width(50));
        if (EditorGUI.EndChangeCheck())
            Repaint();
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (Event.current.type != EventType.Repaint)
            return;

        background.Draw(r, false, false, false, false);

        var style = (TileUIStyle)target;
        EnsurePreview(style);

        if (_previewSprite == null)
        {
            string msg = _previewError ?? "Nothing to preview.";
            GUI.Label(r, msg, new GUIStyle(EditorStyles.centeredGreyMiniLabel) { wordWrap = true });
            return;
        }

        Texture2D tex = _previewSprite.texture;
        const float pad = 16f;
        var area = new Rect(r.x + pad, r.y + pad, r.width - pad * 2, r.height - pad * 2);
        if (area.width <= 0 || area.height <= 0) return;

        float scale = Mathf.Min(area.width / tex.width, area.height / tex.height);
        if (scale >= 1f) scale = Mathf.Floor(scale); // crisp integer scaling for pixel art

        float w = tex.width * scale;
        float h = tex.height * scale;
        var dest = new Rect(area.x + (area.width - w) * 0.5f, area.y + (area.height - h) * 0.5f, w, h);

        GUI.DrawTexture(dest, tex, ScaleMode.StretchToFill, true);
    }

    // ── Building the preview sprite ─────────────────────────────────

    private void EnsurePreview(TileUIStyle style)
    {
        int key = HashCode.Combine(
            style.SpritePixelSize,
            HashCode.Combine(style.Fill, style.EdgeHorizontal, style.EdgeVertical),
            HashCode.Combine(style.OuterCorners, style.InnerCorners),
            _shapeIndex,
            _rotationIndex);

        if (_previewSprite != null && key == _previewKey) return;
        if (_previewSprite == null && _previewError != null && key == _previewKey) return;

        ReleasePreview();
        _previewKey = key;

        if (style.SpritePixelSize <= 0)
        {
            _previewError = "Set Sprite Pixel Size to see a preview.";
            return;
        }

        GameObject go = null;
        try
        {
            // Hidden scratch object so we can reuse the real runtime builder.
            go = new GameObject("TileStylePreview_Temp", typeof(RectTransform))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            var builder = go.AddComponent<TileShapeTextureBuilder>(); // adds Image via RequireComponent

            var shape = new List<Vector2Int>(Shapes[_shapeIndex].cells);
            _previewSprite = builder.Build(shape, style, Rotations[_rotationIndex]);

            if (_previewSprite != null)
            {
                _previewSprite.hideFlags = HideFlags.HideAndDontSave;
                _previewSprite.texture.hideFlags = HideFlags.HideAndDontSave;
                _previewSprite.texture.filterMode = FilterMode.Point;
                _previewError = null;
            }
            else
            {
                _previewError = "Builder returned no sprite.";
            }
        }
        catch (Exception e)
        {
            _previewSprite = null;
            _previewError = "Preview failed: " + e.Message +
                            "\n(Are the style sprites set to Read/Write Enabled?)";
        }
        finally
        {
            // Builder has no ExecuteAlways, so its OnDestroy doesn't run here and
            // the sprite/texture we just took survive. We free them in ReleasePreview.
            if (go != null) DestroyImmediate(go);
        }
    }

    private void ReleasePreview()
    {
        if (_previewSprite != null)
        {
            if (_previewSprite.texture != null) DestroyImmediate(_previewSprite.texture);
            DestroyImmediate(_previewSprite);
        }
        _previewSprite = null;
        _previewError = null;
        _previewKey = 0;
    }
}