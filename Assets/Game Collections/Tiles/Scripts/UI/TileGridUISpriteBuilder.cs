using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tiles.UI
{
    public static class TileGridUISpriteBuilder
    {
        private enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }
        private enum EdgeSide { Top, Bottom, Left, Right }

        // Cached pixel arrays extracted from the style's source sprites.
        private readonly struct SpritePriorityAndPixels
        {
            public readonly (int Priority, Color[] Pixels) Background;
            public readonly (int Priority, Color[] Pixels) OuterCorner;
            public readonly (int Priority, Color[] Pixels) InnerCorner;
            public readonly (int Priority, Color[] Pixels) OuterVertical;
            public readonly (int Priority, Color[] Pixels) OuterHorizontal;
            public readonly (int Priority, Color[] Pixels) InnerVertical;
            public readonly (int Priority, Color[] Pixels) InnerHorizontal;

            public SpritePriorityAndPixels(TileGridUISpriteStyle style)
            {
                Background = (style.BackgroundFillSprite.priority, GetPixels(style.BackgroundFillSprite));
                OuterCorner = (style.OuterCornerSprites.priority, GetPixels(style.OuterCornerSprites));
                InnerCorner = (style.InnerCornerSprites.priority, GetPixels(style.InnerCornerSprites));
                OuterVertical = (style.OuterVerticalSprites.priority, GetPixels(style.OuterVerticalSprites));
                OuterHorizontal = (style.OuterHorizontalSprites.priority, GetPixels(style.OuterHorizontalSprites));
                InnerVertical = (style.InnerVerticalSeperationSprites.priority, GetPixels(style.InnerVerticalSeperationSprites));
                InnerHorizontal = (style.InnerHorizontalSeperationSprites.priority, GetPixels(style.InnerHorizontalSeperationSprites));
            }

            private static Color[] GetPixels(TileGridUISprite sprite)
            {
                if (sprite == null || sprite.Sprite == null) return null;
                Rect r = sprite.Sprite.textureRect;
                return sprite.Sprite.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
            }
        }

        private class DrawOrderItem
        {
            public float Priority;
            public Action StampAction;
        }

        /// <summary>
        /// Builds a sprite that covers <paramref name="size"/> grid cells using <paramref name="gridStyle"/>.
        /// Adjacent cells share their border pixel, matching the (tileSize-1)+1 texture layout.
        /// Stamps are applied in ascending priority order so higher-priority sprites render on top.
        ///
        /// Caller is responsible for destroying the sprite and its texture when done.
        /// </summary>
        public static Sprite GetSprite(Vector2Int size, TileGridUISpriteStyle gridStyle, float uiCellSize)
        {
            if (size.x == 0 || size.y == 0)
            {
                Debug.LogWarning("TileGridUISpriteBuilder: size is zero.");
                return null;
            }

            if (!gridStyle.Validate()) return null;

            SpritePriorityAndPixels sp = new SpritePriorityAndPixels(gridStyle);
            int spritePixelSize = gridStyle.SpritePixelSize;
            int texW = size.x * (spritePixelSize - 1) + 1;
            int texH = size.y * (spritePixelSize - 1) + 1;
            Color[] buffer = new Color[texW * texH];

            var drawOrder = new List<DrawOrderItem>();

            // --- Enqueue full-cell stamps ---
            for (int cx = 0; cx < size.x; cx++)
                for (int cy = 0; cy < size.y; cy++)
                {
                    Vector2Int cell = new Vector2Int(cx, cy);
                    EnqueueFull(drawOrder, buffer, texW, cell, spritePixelSize, sp.Background, 0f);
                    EnqueueFull(drawOrder, buffer, texW, cell, spritePixelSize, sp.InnerVertical, 0.1f);
                    EnqueueFull(drawOrder, buffer, texW, cell, spritePixelSize, sp.InnerHorizontal, 0.1f);
                    EnqueueFull(drawOrder, buffer, texW, cell, spritePixelSize, sp.InnerCorner, 0.2f);
                }

            // --- Enqueue outer edges ---
            for (int cx = 0; cx < size.x; cx++)
            {
                EnqueueOuterEdge(drawOrder, buffer, texW, new Vector2Int(cx, 0), EdgeSide.Bottom, spritePixelSize, sp, 0.3f);
                EnqueueOuterEdge(drawOrder, buffer, texW, new Vector2Int(cx, size.y - 1), EdgeSide.Top, spritePixelSize, sp, 0.3f);
            }
            for (int cy = 0; cy < size.y; cy++)
            {
                EnqueueOuterEdge(drawOrder, buffer, texW, new Vector2Int(0, cy), EdgeSide.Left, spritePixelSize, sp, 0.4f);
                EnqueueOuterEdge(drawOrder, buffer, texW, new Vector2Int(size.x - 1, cy), EdgeSide.Right, spritePixelSize, sp, 0.4f);
            }

            // --- Enqueue outer corners ---
            EnqueueOuterCorner(drawOrder, buffer, texW, new Vector2Int(0, 0), sp.OuterCorner, spritePixelSize, Corner.BottomLeft, 0.5f);
            EnqueueOuterCorner(drawOrder, buffer, texW, new Vector2Int(size.x - 1, 0), sp.OuterCorner, spritePixelSize, Corner.BottomRight, 0.5f);
            EnqueueOuterCorner(drawOrder, buffer, texW, new Vector2Int(0, size.y - 1), sp.OuterCorner, spritePixelSize, Corner.TopLeft, 0.5f);
            EnqueueOuterCorner(drawOrder, buffer, texW, new Vector2Int(size.x - 1, size.y - 1), sp.OuterCorner, spritePixelSize, Corner.TopRight, 0.5f);

            // --- Sort ascending by priority then execute ---
            drawOrder.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            foreach (var item in drawOrder)
                item.StampAction();

            Texture2D texture = new Texture2D(texW, texH, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(buffer);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, texW, texH),
                new Vector2(0.5f, 0.5f),
                spritePixelSize
            );
        }

        // Returns the top-left pixel coordinate of a cell in the shared-border texture.
        private static Vector2Int CellOrigin(Vector2Int cell, int spritePixelSize) =>
            new Vector2Int(cell.x * (spritePixelSize - 1), cell.y * (spritePixelSize - 1));


        #region Enqueue Helpers

        private static void EnqueueFull(
            List<DrawOrderItem> drawOrder,
            Color[] buffer, int texW,
            Vector2Int cell, int spritePixelSize,
            (int Priority, Color[] Pixels) sprite,
            float additionalPriority)
        {
            if (sprite.Pixels == null || sprite.Pixels.Length == 0) return;
            drawOrder.Add(new DrawOrderItem
            {
                Priority = sprite.Priority + additionalPriority,
                StampAction = () => StampFull(buffer, texW, cell, spritePixelSize, sprite.Pixels)
            });
        }

        private static void EnqueueOuterEdge(
            List<DrawOrderItem> drawOrder,
            Color[] buffer, int texW,
            Vector2Int cell, EdgeSide side, int spritePixelSize,
            SpritePriorityAndPixels sp, float additionalPriority)
        {
            int priority = (side == EdgeSide.Top || side == EdgeSide.Bottom)
                ? sp.OuterHorizontal.Priority
                : sp.OuterVertical.Priority;

            drawOrder.Add(new DrawOrderItem
            {
                Priority = priority + additionalPriority,
                StampAction = () => StampOuterEdge(buffer, texW, cell, side, spritePixelSize, sp)
            });
        }

        private static void EnqueueOuterCorner(
            List<DrawOrderItem> drawOrder,
            Color[] buffer, int texW,
            Vector2Int cell,
            (int Priority, Color[] Pixels) sprite,
            int spritePixelSize, Corner corner, float additionalPriority)
        {
            if (sprite.Pixels == null || sprite.Pixels.Length == 0) return;
            drawOrder.Add(new DrawOrderItem
            {
                Priority = sprite.Priority + additionalPriority,
                StampAction = () => StampOuterCorner(buffer, texW, cell, spritePixelSize, sprite.Pixels, corner)
            });
        }

        #endregion


        #region Stamp Methods

        private static void StampFull(Color[] buffer, int texW, Vector2Int cell, int spritePixelSize, Color[] source)
        {
            if (source == null || source.Length == 0) return;

            Vector2Int origin = CellOrigin(cell, spritePixelSize);
            for (int y = 0; y < spritePixelSize; y++)
                for (int x = 0; x < spritePixelSize; x++)
                {
                    Color pixel = source[x + y * spritePixelSize];
                    if (pixel.a > 0f)
                        BlendPixel(buffer, texW, origin.x + x, origin.y + y, pixel);
                }
        }

        private static void StampOuterEdge(Color[] buffer, int texW, Vector2Int cell,
                                            EdgeSide side, int spritePixelSize, SpritePriorityAndPixels sp)
        {
            int half = spritePixelSize / 2;

            Color[] source = (side == EdgeSide.Top || side == EdgeSide.Bottom)
                ? sp.OuterHorizontal.Pixels
                : sp.OuterVertical.Pixels;

            if (source == null || source.Length == 0) return;

            int xStart, xEnd, yStart, yEnd;
            switch (side)
            {
                case EdgeSide.Top: xStart = 0; xEnd = spritePixelSize; yStart = half; yEnd = spritePixelSize; break;
                case EdgeSide.Bottom: xStart = 0; xEnd = spritePixelSize; yStart = 0; yEnd = half; break;
                case EdgeSide.Left: xStart = 0; xEnd = half; yStart = 0; yEnd = spritePixelSize; break;
                case EdgeSide.Right: xStart = half; xEnd = spritePixelSize; yStart = 0; yEnd = spritePixelSize; break;
                default: return;
            }

            Vector2Int origin = CellOrigin(cell, spritePixelSize);
            for (int y = yStart; y < yEnd; y++)
                for (int x = xStart; x < xEnd; x++)
                {
                    Color pixel = source[x + y * spritePixelSize];
                    if (pixel.a > 0f)
                        BlendPixel(buffer, texW, origin.x + x, origin.y + y, pixel);
                }
        }

        private static void StampOuterCorner(Color[] buffer, int texW, Vector2Int cell,
                                              int spritePixelSize, Color[] source, Corner corner)
        {
            bool isRight = corner == Corner.TopRight || corner == Corner.BottomRight;
            bool isTop = corner == Corner.TopLeft || corner == Corner.TopRight;
            int half = spritePixelSize / 2;

            int quadX = isRight ? half + (spritePixelSize.Odd() ? 1 : 0) : 0;
            int quadY = isTop ? half + (spritePixelSize.Odd() ? 1 : 0) : 0;

            Vector2Int origin = CellOrigin(cell, spritePixelSize);
            for (int y = 0; y < half; y++)
                for (int x = 0; x < half; x++)
                {
                    Color pixel = source[(quadX + x) + (quadY + y) * spritePixelSize];
                    if (pixel.a > 0f)
                        BlendPixel(buffer, texW, origin.x + quadX + x, origin.y + quadY + y, pixel);
                }
        }

        #endregion


        private static void BlendPixel(Color[] buffer, int texW, int x, int y, Color source)
        {
            int idx = x + y * texW;
            Color dst = buffer[idx];

            if (dst.a <= 0f) { buffer[idx] = source; return; }

            float a = source.a + dst.a * (1f - source.a);
            Color result = (source * source.a + dst * dst.a * (1f - source.a)) / a;
            result.a = a;
            buffer[idx] = result;
        }
    }
}