using UnityEngine;
using UnityEngine.UIElements;

public class CurveTextureView : VisualElement
{
    Texture2D _tex;
    readonly AnimationCurve[] _curves;
    readonly Color[] _colors;
    readonly int _samples;

    public CurveTextureView(
        AnimationCurve[] curves,
        Color[] colors,
        int samples = 128
    )
    {
        _curves = curves;
        _colors = colors;
        _samples = samples;

        // Let it stretch
        style.flexGrow = 1;
        style.height = 100;

        // When first added to panel or resized, rebuild
        RegisterCallback<AttachToPanelEvent>(e => Rebuild());
        RegisterCallback<GeometryChangedEvent>(e => Rebuild());
    }

    void Rebuild()
    {
        int w = Mathf.FloorToInt(resolvedStyle.width);
        int h = Mathf.FloorToInt(resolvedStyle.height);
        if (w < 2 || h < 2) return;

        // (Re)create texture if size changed
        if (_tex == null || _tex.width != w || _tex.height != h)
        {
            _tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            _tex.hideFlags = HideFlags.HideAndDontSave;
        }

        // Clear background
        var bgColor = new Color32(30, 30, 30, 255);
        var pixels = new Color32[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = bgColor;
        _tex.SetPixels32(pixels);

        // Determine min/max across all curves
        float vMin = 0f, vMax = 1f;
        foreach (var c in _curves)
            if (c != null)
                foreach (var k in c.keys)
                {
                    vMin = Mathf.Min(vMin, k.value);
                    vMax = Mathf.Max(vMax, k.value);
                }
        float vr = Mathf.Max(1e-5f, vMax - vMin);

        // Draw each curve with Bresenham
        for (int ci = 0; ci < _curves.Length; ci++)
        {
            var curve = _curves[ci];
            var col = (_colors != null && ci < _colors.Length) ? _colors[ci] : Color.white;
            if (curve == null || curve.length == 0) continue;

            Vector2 prev = Vector2.zero, curr = Vector2.zero;
            for (int i = 0; i <= _samples; i++)
            {
                float t = i / (float)_samples;
                float v = curve.Evaluate(t);
                curr.x = t * (w - 1);
                curr.y = (v - vMin) / vr * (h - 1);

                if (i > 0)
                    DrawLine(
                        _tex,
                        (int)prev.x, (h - 1) - (int)prev.y,
                        (int)curr.x, (h - 1) - (int)curr.y,
                        col
                    );

                prev = curr;
            }
        }

        _tex.Apply();
        style.backgroundImage = new StyleBackground(_tex);
    }

    // Simple Bresenham line drawer
    static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2, e2;

        while (true)
        {
            tex.SetPixel(x0, y0, col);
            if (x0 == x1 && y0 == y1) break;
            e2 = err;
            if (e2 > -dx) { err -= dy; x0 += sx; }
            if (e2 < dy) { err += dx; y0 += sy; }
        }
    }
}
