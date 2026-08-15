using System;
using System.Collections.Generic;
using UnityEngine;

public class Stamp
{
    public readonly Color[] Colors;
    private int FlatIndex(int x, int y, int? width = null) => y * (width.HasValue ? width.Value : Width) + x;

    public readonly Vector4 Borders;

    [Flags]
    public enum BaseSubStamps
    {
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 4,
        CenterLeft = 8,
        Center = 16,
        CenterRight = 32,
        BottomLeft = 64,
        BottomCenter = 128,
        BottomRight = 256,
    }

    public enum ClockwiseRotations
    {
        None = 0,
        Deg90 = 90,
        Deg180 = 180,
        Deg270 = 270,
    }

    private Dictionary<BaseSubStamps, Stamp> subStamps;
    public IReadOnlyDictionary<BaseSubStamps, Stamp> SubStamps => subStamps;

    private Dictionary<ClockwiseRotations, Stamp> rotatedStamps;
    public IReadOnlyDictionary<ClockwiseRotations, Stamp> RotatedStamps => rotatedStamps;

    public Color this[int x, int y]
    {
        get
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException();
            return Colors[FlatIndex(x, y)];
        }
        set
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException();
            Colors[FlatIndex(x, y)] = value;
        }
    }

    public readonly int Width;
    public int Height => Colors.Length / Width;

    public Stamp(Color[] colors, int width, Vector4 borders = default, bool createRotationStamps = false)
    {
        if (colors == null || colors.Length == 0)
            throw new ArgumentException("Color array is null or empty.", nameof(colors));
        if (width <= 0)
            throw new ArgumentException("Width must be greater than zero.", nameof(width));
        if (colors.Length % width != 0)
            throw new ArgumentException("Color array and width do not match to make a rectangle/square.", nameof(width));
        Colors = colors;
        Width = width;
        Borders = borders;
        if (borders != default)
            CreateBaseSubStamps(borders);

        if (createRotationStamps)
            CreateRotatedSubStamps();
    }

    public Stamp(Color[,] colors, Vector4 borders = default, bool createRotationStamps = false)
    {
        if (colors == null || colors.GetLength(0) == 0 || colors.GetLength(1) == 0)
            throw new ArgumentException("Color array is null or empty.", nameof(colors));

        Width = colors.GetLength(0);
        int height = colors.GetLength(1);
        Borders = borders;
        Color[] colorArray = new Color[Width * height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = y * Width + x;
                colorArray[index] = colors[x, y];
            }
        }

        Colors = colorArray;

        if (borders != default)
            CreateBaseSubStamps(borders);

        if (createRotationStamps)
            CreateRotatedSubStamps();
    }

    public Stamp(Sprite sprite, bool createRotationStamps = false)
    {
        if (sprite == null)
            throw new ArgumentNullException(nameof(sprite));

        Rect spriteRect = sprite.textureRect;

        Color[] color = sprite.texture.GetPixels(
            (int)spriteRect.x,
            (int)spriteRect.y,
            (int)spriteRect.width,
            (int)spriteRect.height);

        Colors = color;
        Width = (int)spriteRect.width;
        Borders = sprite.border;

        if (sprite.border != default)
            CreateBaseSubStamps(sprite.border);

        if (createRotationStamps)
            CreateRotatedSubStamps();
    }

    private void CreateBaseSubStamps(Vector4 border)
    {
        subStamps = new Dictionary<BaseSubStamps, Stamp>();

        int left = (int)border.x;
        int bottom = (int)border.y;
        int right = (int)border.z;
        int top = (int)border.w;

        int centerWidth = Width - left - right;
        int centerHeight = Height - top - bottom;

        if (left < 0 || right < 0 || top < 0 || bottom < 0 || centerWidth < 0 || centerHeight < 0)
            throw new ArgumentException("Border values exceed the size of the stamp.", nameof(border));

        void TryAdd(BaseSubStamps key, RectInt region)
        {
            if (region.width <= 0 || region.height <= 0) return; // no border on this edge, nothing to slice
            subStamps.Add(key, GetSubStamp(region, true));
        }

        TryAdd(BaseSubStamps.BottomLeft, new RectInt(0, 0, left, bottom));
        TryAdd(BaseSubStamps.BottomCenter, new RectInt(left, 0, centerWidth, bottom));
        TryAdd(BaseSubStamps.BottomRight, new RectInt(Width - right, 0, right, bottom));

        TryAdd(BaseSubStamps.CenterLeft, new RectInt(0, bottom, left, centerHeight));
        TryAdd(BaseSubStamps.Center, new RectInt(left, bottom, centerWidth, centerHeight));
        TryAdd(BaseSubStamps.CenterRight, new RectInt(Width - right, bottom, right, centerHeight));

        TryAdd(BaseSubStamps.TopLeft, new RectInt(0, Height - top, left, top));
        TryAdd(BaseSubStamps.TopCenter, new RectInt(left, Height - top, centerWidth, top));
        TryAdd(BaseSubStamps.TopRight, new RectInt(Width - right, Height - top, right, top));
    }

    public void CreateRotatedSubStamps()
    {
        rotatedStamps = new Dictionary<ClockwiseRotations, Stamp>();
        rotatedStamps.Add(ClockwiseRotations.None, this);
        rotatedStamps.Add(ClockwiseRotations.Deg90, CreateRotatedSubStamp(ClockwiseRotations.Deg90));
        rotatedStamps.Add(ClockwiseRotations.Deg180, CreateRotatedSubStamp(ClockwiseRotations.Deg180));
        rotatedStamps.Add(ClockwiseRotations.Deg270, CreateRotatedSubStamp(ClockwiseRotations.Deg270));
    }

    /// <summary>
    /// Returns a copy of this stamp with its pixel content rotated (this stamp is
    /// unchanged). Unlike <see cref="RotatedStamps"/>, this works on ANY stamp on
    /// demand — including a small leaf piece pulled from GetSubStamp/SubStamps —
    /// not just ones pre-built with createRotationStamps: true.
    /// </summary>
    public Stamp Rotated(ClockwiseRotations rotation)
        => rotation == ClockwiseRotations.None ? this : CreateRotatedSubStamp(rotation);

    private Stamp CreateRotatedSubStamp(ClockwiseRotations rotation)
    {
        Color[] newColors = new Color[Width * Height];
        int newWidth;

        switch (rotation)
        {
            case ClockwiseRotations.Deg90:
                newWidth = Height;
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                    {
                        int nx = y;
                        int ny = Width - 1 - x;
                        newColors[ny * newWidth + nx] = this[x, y];
                    }
                break;

            case ClockwiseRotations.Deg180:
                newWidth = Width;
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                    {
                        int nx = Width - 1 - x;
                        int ny = Height - 1 - y;
                        newColors[ny * newWidth + nx] = this[x, y];
                    }
                break;
            case ClockwiseRotations.Deg270:
                newWidth = Height;
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                    {
                        int nx = Height - 1 - y;
                        int ny = x;
                        newColors[ny * newWidth + nx] = this[x, y];
                    }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(rotation));
        }

        // Slice using the ORIGINAL border, unrotated — for a symmetric border (corners)
        // this is provably equivalent to a rotated border; for a leaf piece with no
        // border (Borders == default) it's a no-op either way.
        return new Stamp(newColors, newWidth, Borders);
    }

    public Stamp Merge(Stamp other, Vector2Int offset)
    {
        Color[] otherColors = (Color[])other.Colors.Clone();

        StampFull(otherColors, other.Width, offset);

        return new Stamp(otherColors, other.Width);
    }

    public Color GetPixel(int x, int y)
    {
        return this[x, y];
    }

    public bool TrySetPixel(int x, int y, Color color)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
            return false;

        this[x, y] = color;

        return true;
    }

    public Texture2D ToTexture2D()
    {
        var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels(Colors);
        texture.Apply();
        return texture;
    }

    public Stamp GetSubStamp(RectInt region, bool cropToRegion)
    {
        if (region.x < 0 || region.y < 0 || region.x + region.width > Width || region.y + region.height > Height)
            throw new ArgumentOutOfRangeException(nameof(region), "Region is outside the bounds of the stamp.");

        int subWidth = region.width;
        int subHeight = region.height;

        Color[] subStampColors;

        subStampColors = cropToRegion ? new Color[subWidth * subHeight] : new Color[Width * Height];

        for (int x = 0; x < subWidth; x++)
        {
            for (int y = 0; y < subHeight; y++)
            {
                int ix = region.x + x;
                int iy = region.y + y;

                Color pixel = GetPixel(ix, iy);
                int index = cropToRegion ? FlatIndex(x, y, subWidth) : FlatIndex(ix, iy, Width);
                subStampColors[index] = pixel;
            }
        }

        return new Stamp(subStampColors, cropToRegion ? subWidth : Width);
    }

    public void StampFull(Color[] buffer, int textureWidth, Vector2Int offset)
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                Color pixel = this[x, y];
                if (pixel.a <= 0f) continue;
                WritePixel(buffer, textureWidth, offset.x + x, offset.y + y, pixel);
            }
    }

    private void WritePixel(Color[] buffer, int textureWidth, int x, int y, Color src)
    {
        if (x < 0 || x >= textureWidth || y < 0)
            return; // out of bounds, clip
        int idx = x + y * textureWidth;
        if (idx < 0 || idx >= buffer.Length)
            return;

        Color dst = buffer[idx];

        if (dst.a <= 0f)
        {
            buffer[idx] = src;
            return;
        }

        float a = src.a + dst.a * (1f - src.a);
        Color result = (src * src.a + dst * dst.a * (1f - src.a)) / a;
        result.a = a;
        buffer[idx] = result;
    }

}