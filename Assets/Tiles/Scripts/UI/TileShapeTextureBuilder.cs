using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tiles
{
    /// <summary>
    /// Builds a Texture2D for any tile shape and assigns it to a RawImage.
    /// Call Build(shape) once when the tile is created or its shape changes.
    ///
    /// Sprite layout — every sprite must be cellPixelSize × cellPixelSize:
    ///
    ///   fill            — solid interior, no border pixels
    ///   edgeHorizontal  — border on top AND bottom, transparent left/right columns
    ///   edgeVertical    — border on left AND right, transparent top/bottom rows
    ///   outerCorners    — all four outer corner pieces in one sprite:
    ///                       top-left quadrant     = TopLeft corner piece
    ///                       top-right quadrant    = TopRight corner piece
    ///                       bottom-left quadrant  = BottomLeft corner piece
    ///                       bottom-right quadrant = BottomRight corner piece
    ///                     non-corner pixels transparent
    ///   innerCorners    — same quadrant layout as outerCorners, for concave notches
    ///
    /// Edge sprites use the same half-cell quadrant scheme:
    ///   edgeHorizontal top half    (y >= half) ? exposed Top edge
    ///   edgeHorizontal bottom half (y  < half) ? exposed Bottom edge
    ///   edgeVertical   left half   (x  < half) ? exposed Left edge
    ///   edgeVertical   right half  (x >= half) ? exposed Right edge
    /// Only the relevant half is stamped per exposed edge.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class TileShapeTextureBuilder : MonoBehaviour
    {
        // ?? Inspector ????????????????????????????????????????????????????????????
        [Header("Pixel Settings")]
        [Tooltip("Pixel width/height of every sprite. All sprites must be this size.")]
        [SerializeField] private int cellPixelSize = 13;

        [Tooltip("How many UI units wide one cell should appear on screen.")]
        [SerializeField] private float uiCellSize = 32f;

        [Header("Sprites — all must be cellPixelSize × cellPixelSize")]
        [Tooltip("Solid interior fill, no border.")]
        [SerializeField] private Sprite fill;

        [Tooltip("Border on top and bottom. Top half of sprite = top edge, bottom half = bottom edge.")]
        [SerializeField] private Sprite edgeHorizontal;

        [Tooltip("Border on left and right. Left half of sprite = left edge, right half = right edge.")]
        [SerializeField] private Sprite edgeVertical;

        [Tooltip("All four outer corner pieces. Each quadrant of the sprite is one corner.")]
        [SerializeField] private Sprite outerCorners;

        [Tooltip("All four inner corner caps. Each quadrant of the sprite is one corner.")]
        [SerializeField] private Sprite innerCorners;

        [Tooltip("Alpha threshold below which pixels don't register raycasts (0–1).")]
        [SerializeField][Range(0f, 1f)] private float alphaHitThreshold = 0.1f;

        // ?? Runtime ??????????????????????????????????????????????????????????????
        private Texture2D _texture;
        private Sprite _generatedSprite;
        private Image _image;

        private Color[] _fillPixels;
        private Color[] _edgeHPixels;
        private Color[] _edgeVPixels;
        private Color[] _outerCornerPixels;
        private Color[] _innerCornerPixels;

        // Working buffer — all passes write here, flushed once at the end
        private Color[] _working;
        private int _texW;

        // ?? Enums ????????????????????????????????????????????????????????????????
        private enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }
        private enum EdgeSide { Top, Bottom, Left, Right }

        // ?????????????????????????????????????????????????????????????????????????
        // Public API
        // ?????????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Generates and applies the texture for <paramref name="shape"/>.
        /// Safe to call multiple times — the previous texture is destroyed first.
        /// </summary>
        public void Build(List<Vector2Int> shape)
        {
            if (shape == null || shape.Count == 0)
            {
                Debug.LogWarning($"[TileShapeTextureBuilder] {name}: shape is empty.");
                return;
            }

            if (!ValidateSprites()) return;

            CachePixels();
            DestroyTexture();

            Vector2Int boundsMin = BoundsMin(shape);
            Vector2Int boundsMax = BoundsMax(shape);
            Vector2Int boundsSize = boundsMax - boundsMin + Vector2Int.one;

            int texW = boundsSize.x * cellPixelSize;
            int texH = boundsSize.y * cellPixelSize;

            _texture = new Texture2D(texW, texH, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            _texW = texW;
            _working = new Color[texW * texH]; // transparent by default

            var shapeSet = new HashSet<Vector2Int>(shape);

            foreach (Vector2Int cell in shape)
            {
                Vector2Int o = Offset(cell, boundsMin);

                bool hasTop = shapeSet.Contains(cell + Vector2Int.up);
                bool hasBottom = shapeSet.Contains(cell + Vector2Int.down);
                bool hasLeft = shapeSet.Contains(cell + Vector2Int.left);
                bool hasRight = shapeSet.Contains(cell + Vector2Int.right);

                bool exposedTop = !hasTop;
                bool exposedBottom = !hasBottom;
                bool exposedLeft = !hasLeft;
                bool exposedRight = !hasRight;

                // ?? Pass 1: fill ?????????????????????????????????????????????????
                StampFull(o, _fillPixels);

                // ?? Pass 2: edges ????????????????????????????????????????????????
                // Only stamp the half of the edge sprite facing the exposed side.
                if (exposedTop) StampEdge(o, EdgeSide.Top);
                if (exposedBottom) StampEdge(o, EdgeSide.Bottom);
                if (exposedLeft) StampEdge(o, EdgeSide.Left);
                if (exposedRight) StampEdge(o, EdgeSide.Right);
            }

            foreach (Vector2Int cell in shape)
            {
                Vector2Int o = Offset(cell, boundsMin);

                bool hasTop = shapeSet.Contains(cell + Vector2Int.up);
                bool hasBottom = shapeSet.Contains(cell + Vector2Int.down);
                bool hasLeft = shapeSet.Contains(cell + Vector2Int.left);
                bool hasRight = shapeSet.Contains(cell + Vector2Int.right);

                bool exposedTop = !hasTop;
                bool exposedBottom = !hasBottom;
                bool exposedLeft = !hasLeft;
                bool exposedRight = !hasRight;

                // ?? Pass 3: outer corners ????????????????????????????????????????
                // Only stamp the quadrant of outerCorners matching this corner.
                if (exposedTop && exposedLeft) StampCorner(o, _outerCornerPixels, Corner.TopLeft);
                if (exposedTop && exposedRight) StampCorner(o, _outerCornerPixels, Corner.TopRight);
                if (exposedBottom && exposedLeft) StampCorner(o, _outerCornerPixels, Corner.BottomLeft);
                if (exposedBottom && exposedRight) StampCorner(o, _outerCornerPixels, Corner.BottomRight);

                // ?? Pass 4: inner corners ????????????????????????????????????????
                // Concave notch: two filled neighbours whose shared diagonal is missing.
                if (hasRight && hasTop && !shapeSet.Contains(cell + Vector2Int.right + Vector2Int.up))
                    StampCorner(o, _innerCornerPixels, Corner.TopRight);
                if (hasLeft && hasTop && !shapeSet.Contains(cell + Vector2Int.left + Vector2Int.up))
                    StampCorner(o, _innerCornerPixels, Corner.TopLeft);
                if (hasRight && hasBottom && !shapeSet.Contains(cell + Vector2Int.right + Vector2Int.down))
                    StampCorner(o, _innerCornerPixels, Corner.BottomRight);
                if (hasLeft && hasBottom && !shapeSet.Contains(cell + Vector2Int.left + Vector2Int.down))
                    StampCorner(o, _innerCornerPixels, Corner.BottomLeft);
            }

            _texture.SetPixels(_working);
            _texture.Apply();

            if (_image == null)
                _image = GetComponent<Image>();


            _generatedSprite = Sprite.Create(
                _texture,
                new Rect(0, 0, _texture.width, _texture.height),
                new Vector2(0.5f, 0.5f), // pivot centre
                cellPixelSize             // pixels per unit
            );

            _image.sprite = _generatedSprite;
            _image.alphaHitTestMinimumThreshold = alphaHitThreshold;

            float scale = uiCellSize / cellPixelSize;
            GetComponent<RectTransform>().sizeDelta = new Vector2(texW, texH) * scale;
        }

        // ?????????????????????????????????????????????????????????????????????????
        // Stamping
        // ?????????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Blits an entire cellPixelSize×cellPixelSize sprite into the working buffer.
        /// Used for the fill pass. Transparent source pixels are skipped.
        /// </summary>
        private void StampFull(Vector2Int cellOffset, Color[] src)
        {
            int s = cellPixelSize;

            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    Color pixel = src[x + y * s];
                    if (pixel.a <= 0f) continue;
                    WritePixel(cellOffset.x + x, cellOffset.y + y, pixel);
                }
        }

        /// <summary>
        /// Stamps one half of an edge sprite — only the half facing the exposed side.
        ///
        /// edgeHorizontal:
        ///   Top    ? rows y >= half  (top half of sprite)
        ///   Bottom ? rows y  < half  (bottom half of sprite)
        /// edgeVertical:
        ///   Left   ? cols x  < half  (left half of sprite)
        ///   Right  ? cols x >= half  (right half of sprite)
        ///
        /// Pixels are written to the same (x,y) position in the cell so the border
        /// lands on the correct edge of the output texture.
        /// </summary>
        private void StampEdge(Vector2Int cellOffset, EdgeSide side)
        {
            int s = cellPixelSize;
            int half = s / 2;

            Color[] src = (side == EdgeSide.Top || side == EdgeSide.Bottom)
                ? _edgeHPixels
                : _edgeVPixels;

            int xStart, xEnd, yStart, yEnd;

            switch (side)
            {
                case EdgeSide.Top: xStart = 0; xEnd = s; yStart = half; yEnd = s; break;
                case EdgeSide.Bottom: xStart = 0; xEnd = s; yStart = 0; yEnd = half; break;
                case EdgeSide.Left: xStart = 0; xEnd = half; yStart = 0; yEnd = s; break;
                case EdgeSide.Right: xStart = half; xEnd = s; yStart = 0; yEnd = s; break;
                default: return;
            }

            for (int y = yStart; y < yEnd; y++)
                for (int x = xStart; x < xEnd; x++)
                {
                    Color pixel = src[x + y * s];
                    if (pixel.a <= 0f) continue;
                    WritePixel(cellOffset.x + x, cellOffset.y + y, pixel);
                }
        }

        /// <summary>
        /// Stamps one quadrant of a corners sprite (outerCorners or innerCorners).
        ///
        /// Quadrant layout in the sprite (each quadrant is half×half pixels):
        ///
        ///   TopLeft     ? x: 0..half,  y: half..s   (top-left    of sprite)
        ///   TopRight    ? x: half..s,  y: half..s   (top-right   of sprite)
        ///   BottomLeft  ? x: 0..half,  y: 0..half   (bottom-left  of sprite)
        ///   BottomRight ? x: half..s,  y: 0..half   (bottom-right of sprite)
        ///
        /// The quadrant is written to the matching position in the destination cell
        /// so corner pixels land in the correct corner of the output texture.
        /// </summary>
        private void StampCorner(Vector2Int cellOffset, Color[] src, Corner corner)
        {
            int s = cellPixelSize;
            int half = s / 2;

            int srcX = (corner == Corner.TopRight || corner == Corner.BottomRight) ? half : 0;
            int srcY = (corner == Corner.TopLeft || corner == Corner.TopRight) ? half : 0;

            if (cellPixelSize.Odd() && (corner == Corner.BottomRight || corner == Corner.TopRight))
                srcX += 1;

            if (cellPixelSize.Odd() && (corner == Corner.TopLeft || corner == Corner.TopRight))
                srcY += 1;

            // Destination in the cell matches the source quadrant position
            int dstX = srcX;
            int dstY = srcY;

            for (int y = 0; y < half; y++)
                for (int x = 0; x < half; x++)
                {
                    Color pixel = src[(srcX + x) + (srcY + y) * s];
                    if (pixel.a <= 0f) continue;
                    WritePixel(cellOffset.x + dstX + x, cellOffset.y + dstY + y, pixel);
                }
        }

        /// <summary>
        /// Alpha-composites a pixel into the working buffer (src over dst).
        /// </summary>
        private void WritePixel(int x, int y, Color src)
        {
            int idx = x + y * _texW;
            Color dst = _working[idx];

            if (dst.a <= 0f)
            {
                _working[idx] = src;
                return;
            }

            float a = src.a + dst.a * (1f - src.a);
            Color result = (src * src.a + dst * dst.a * (1f - src.a)) / a;
            result.a = a;
            _working[idx] = result;
        }

        // ?????????????????????????????????????????????????????????????????????????
        // Pixel caching
        // ?????????????????????????????????????????????????????????????????????????

        private void CachePixels()
        {
            _fillPixels = GetSpritePixels(fill);
            _edgeHPixels = GetSpritePixels(edgeHorizontal);
            _edgeVPixels = GetSpritePixels(edgeVertical);
            _outerCornerPixels = GetSpritePixels(outerCorners);
            _innerCornerPixels = GetSpritePixels(innerCorners);
        }

        /// <summary>
        /// Extracts pixels from a sprite's sub-rect in its texture atlas.
        /// The texture must have Read/Write Enabled in its import settings.
        /// </summary>
        private static Color[] GetSpritePixels(Sprite sprite)
        {
            Rect rect = sprite.textureRect;
            return sprite.texture.GetPixels(
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height);
        }

        // ?????????????????????????????????????????????????????????????????????????
        // Bounds & offset helpers
        // ?????????????????????????????????????????????????????????????????????????

        private Vector2Int Offset(Vector2Int cell, Vector2Int boundsMin)
            => (cell - boundsMin) * cellPixelSize;

        private static Vector2Int BoundsMin(List<Vector2Int> shape)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var c in shape)
            {
                if (c.x < minX) minX = c.x;
                if (c.y < minY) minY = c.y;
            }
            return new Vector2Int(minX, minY);
        }

        private static Vector2Int BoundsMax(List<Vector2Int> shape)
        {
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var c in shape)
            {
                if (c.x > maxX) maxX = c.x;
                if (c.y > maxY) maxY = c.y;
            }
            return new Vector2Int(maxX, maxY);
        }

        // ?????????????????????????????????????????????????????????????????????????
        // Validation & lifecycle
        // ?????????????????????????????????????????????????????????????????????????

        private bool ValidateSprites()
        {
            bool ok = true;
            ok &= AssertSprite(fill, nameof(fill));
            ok &= AssertSprite(edgeHorizontal, nameof(edgeHorizontal));
            ok &= AssertSprite(edgeVertical, nameof(edgeVertical));
            ok &= AssertSprite(outerCorners, nameof(outerCorners));
            ok &= AssertSprite(innerCorners, nameof(innerCorners));
            return ok;
        }

        private bool AssertSprite(Sprite sprite, string fieldName)
        {
            if (sprite != null) return true;
            Debug.LogError($"[TileShapeTextureBuilder] {name}: '{fieldName}' is not assigned.");
            return false;
        }

        private void DestroyTexture()
        {
            if (_generatedSprite != null)
            {
                Destroy(_generatedSprite);
                _generatedSprite = null;
            }
            if (_texture != null)
            {
                Destroy(_texture);
                _texture = null;
            }
        }

        private void OnDestroy() => DestroyTexture();
    }
}