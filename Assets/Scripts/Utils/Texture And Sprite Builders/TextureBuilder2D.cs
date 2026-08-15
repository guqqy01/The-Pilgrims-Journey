using UnityEngine;

public static class TextureBuilder2D
{
    public static Color[] GetSpritePixels(Sprite sprite)
    {
        if (sprite == null) return null;

        Rect rect = sprite.textureRect;
        return sprite.texture.GetPixels(
            (int)rect.x,
            (int)rect.y,
            (int)rect.width,
            (int)rect.height);
    }

    public static void WritePixel(ref Color[] working, int textureWidth, int x, int y, Color src)
    {
        int idx = x + y * textureWidth;
        Color dst = working[idx];

        if (dst.a <= 0f)
        {
            working[idx] = src;
            return;
        }

        float a = src.a + dst.a * (1f - src.a);
        Color result = (src * src.a + dst * dst.a * (1f - src.a)) / a;
        result.a = a;
        working[idx] = result;
    }

    private static Texture2D CreateTextureFromBuffer(Color[] buffer, int texW, int texH)
    {
        var texture = new Texture2D(texW, texH, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels(buffer);
        texture.Apply();
        return texture;
    }
}

public class TextureInfo2D
{
    public int width;
    public int height;
}