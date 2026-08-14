using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tiles
{
    /// <summary>
    /// Builds a Texture2D for any tile shape and assigns it to a RawImage.
    /// Call Build(shape) once when the tile is created or its shape changes,
    /// or GetAllSpriteRotations(shape) to get one sprite per 90° rotation
    /// (useful when the art has baked-in lighting/shading that only looks
    /// correct if it's redrawn per orientation rather than rotated as a whole).
    ///
    /// Each of the sprites below is optional — any that aren't assigned on
    /// TileUIStyle are simply skipped, so e.g. a style with no OuterCorners
    /// sprite will render without outer corner pieces. Every sprite that is
    /// assigned must be cellPixelSize × cellPixelSize:
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
        [Tooltip("How many UI units wide one cell should appear on screen.")]
        [SerializeField] private float uiCellSize = 32f;

        [Tooltip("Alpha threshold below which pixels don't register raycasts (0–1).")]
        [SerializeField][Range(0f, 1f)] private float alphaHitThreshold = 0.1f;

        [Tooltip("How many source pixels the highlight is grown outward from the tile's actual silhouette.")]
        [SerializeField][Range(0, 8)] private int highlightPaddingPixels = 1;

        [SerializeField] private Image image;
        [SerializeField] private Image highlightImage;

        private static readonly int[] RotationAngles = { 0, 90, 180, 270 };

        // Every Texture2D/Sprite this component has ever created, so OnDestroy
        // can clean all of it up — including sprites handed out via
        // GetAllSpriteRotations that outlive any single Build() call.
        private readonly List<Object> _ownedResources = new();

        // The texture/sprite currently shown on `image` / `highlightImage`.
        // Build() destroys and replaces these each time it's called; sprites
        // produced by GetAllSpriteRotations are tracked in _ownedResources
        // only, so building one rotation never destroys another.
        private Texture2D _currentTexture;
        private Sprite _currentSprite;
        private Texture2D _currentHighlightTexture;
        private Sprite _currentHighlightSprite;

        // Cache populated by GetAllSpriteRotations; ChangeSprite reads from this.
        private Dictionary<int, Sprite> _rotationSprites;

        private Color[] _fillPixels;
        private Color[] _edgeHPixels;
        private Color[] _edgeVPixels;
        private Color[] _outerCornerPixels;
        private Color[] _innerCornerPixels;

        private TileUIStyle _uiStyle;

        // Working buffer — all stamping writes here, flushed once at the end.
        // Reused across sequential BuildSprite calls; each call fully
        // completes before the next starts, so this is safe.
        private Color[] _working;
        private int _texW;

        private enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }
        private enum EdgeSide { Top, Bottom, Left, Right }

        /// <summary>
        /// Which of a cell's 8 neighbours are part of the shape, and which
        /// of the cell's 4 sides are therefore exposed (border-facing).
        /// </summary>
        private readonly struct CellNeighbors
        {
            public readonly bool Top, Bottom, Left, Right;
            public readonly bool TopLeft, TopRight, BottomLeft, BottomRight;

            public CellNeighbors(Vector2Int cell, HashSet<Vector2Int> shapeSet)
            {
                Top = shapeSet.Contains(cell + Vector2Int.up);
                Bottom = shapeSet.Contains(cell + Vector2Int.down);
                Left = shapeSet.Contains(cell + Vector2Int.left);
                Right = shapeSet.Contains(cell + Vector2Int.right);

                TopLeft = shapeSet.Contains(cell + Vector2Int.up + Vector2Int.left);
                TopRight = shapeSet.Contains(cell + Vector2Int.up + Vector2Int.right);
                BottomLeft = shapeSet.Contains(cell + Vector2Int.down + Vector2Int.left);
                BottomRight = shapeSet.Contains(cell + Vector2Int.down + Vector2Int.right);
            }

            public bool ExposedTop => !Top;
            public bool ExposedBottom => !Bottom;
            public bool ExposedLeft => !Left;
            public bool ExposedRight => !Right;
        }

        // ?????????????????????????????????????????????????????????????????
        // Public API
        // ?????????????????????????????????????????????????????????????????

        /// <summary>
        /// Generates and applies the texture for <paramref name="shape"/>, replacing
        /// whatever this component previously displayed. Safe to call repeatedly —
        /// the previous texture/sprite is destroyed once the new one is ready.
        /// Returns the generated sprite (null if the shape was empty).
        /// </summary>
        public Sprite Build(List<Vector2Int> shape, TileUIStyle uiStyle)
        {
            _uiStyle = uiStyle;
            CachePixels();

            Sprite sprite = BuildSprite(shape, out Texture2D texture);
            if (sprite == null) return null;

            ReplaceCurrent(ref _currentTexture, texture);
            ReplaceCurrent(ref _currentSprite, sprite);
            AssignMainSprite(sprite);

            BuildAndAssignHighlight(texture);

            return sprite;
        }

        /// <summary>
        /// Builds one independent sprite per 90° rotation of <paramref name="shape"/>, and
        /// caches them so <see cref="ChangeSprite"/> can switch between them later. Unlike
        /// rotating a single sprite in the UI, this redraws each orientation from scratch, so
        /// baked-in lighting/shading on the tile art stays correct instead of rotating along
        /// with the tile. Calling this again replaces the cache — see <see cref="ChangeSprite"/>
        /// for the caveat about doing that while a rotation is actively displayed.
        /// </summary>
        public Dictionary<int, Sprite> GetAllSpriteRotations(List<Vector2Int> shape, TileUIStyle uiStyle)
        {
            _uiStyle = uiStyle;
            CachePixels();

            ReleaseCachedRotations();

            var rotations = new Dictionary<int, Sprite>();
            foreach (int angle in RotationAngles)
            {
                List<Vector2Int> rotatedShape = angle == 0 ? shape : shape.Rotate(angle);
                Sprite sprite = BuildSprite(rotatedShape, out _);
                if (sprite != null)
                    rotations[angle] = sprite;
            }
            _rotationSprites = rotations;
            return rotations;
        }

        /// <summary>
        /// Switches `image` to the cached sprite for <paramref name="rotation"/> (0/90/180/270),
        /// resizing the RectTransform to match — a 90°/270° sprite can have swapped width/height
        /// versus 0°/180°. Requires <see cref="GetAllSpriteRotations"/> to have been called first.
        /// Note: calling GetAllSpriteRotations again destroys the previous rotation set, including
        /// whichever one is currently displayed, so avoid regenerating rotations while relying on
        /// this method to switch between them.
        /// </summary>
        public void ChangeSprite(int rotation)
        {
            if (rotation != 0 && rotation != 90 && rotation != 180 && rotation != 270)
            {
                Debug.LogWarning($"[TileShapeTextureBuilder] {name}: {rotation} is not a valid rotation (expected 0, 90, 180, or 270).");
                return;
            }

            if (_rotationSprites == null || !_rotationSprites.TryGetValue(rotation, out Sprite sprite) || sprite == null)
            {
                Debug.LogWarning($"[TileShapeTextureBuilder] {name}: no cached sprite for {rotation}° — call GetAllSpriteRotations first.");
                return;
            }
            AssignMainSprite(sprite);
        }

        /// <summary>Destroys and untracks any previously cached rotation sprites.</summary>
        private void ReleaseCachedRotations()
        {
            if (_rotationSprites == null) return;

            foreach (Sprite sprite in _rotationSprites.Values)
            {
                if (sprite == null) continue;

                if (sprite.texture != null)
                {
                    _ownedResources.Remove(sprite.texture);
                    Destroy(sprite.texture);
                }

                _ownedResources.Remove(sprite);
                Destroy(sprite);
            }

            _rotationSprites = null;
        }

        private bool IsValidShape(List<Vector2Int> shape)
        {
            if (shape != null && shape.Count > 0) return true;
            Debug.LogWarning($"[TileShapeTextureBuilder] {name}: shape is empty.");
            return false;
        }

        // ?????????????????????????????????????????????????????????????????
        // Core build
        // ?????????????????????????????????????????????????????????????????

        /// <summary>
        /// Builds and tracks a new texture + sprite for <paramref name="shape"/>. Assumes
        /// CachePixels() has already been called for the current _uiStyle. Pure in the
        /// sense that it never touches `image`, `highlightImage`, or the "current" fields —
        /// callers decide what to do with the result.
        /// </summary>
        private Sprite BuildSprite(List<Vector2Int> shape, out Texture2D texture)
        {
            texture = null;
            if (!IsValidShape(shape)) return null;

            Vector2Int boundsMin = BoundsMin(shape);
            Vector2Int boundsSize = BoundsMax(shape) - boundsMin + Vector2Int.one;
            int texW = boundsSize.x * _uiStyle.SpritePixelSize;
            int texH = boundsSize.y * _uiStyle.SpritePixelSize;

            _texW = texW;
            _working = new Color[texW * texH]; // transparent by default

            var shapeSet = new HashSet<Vector2Int>(shape);
            foreach (Vector2Int cell in shape)
                StampCell(cell, boundsMin, shapeSet);

            texture = CreateTextureFromWorkingBuffer(texW, texH);
            Track(texture);

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texW, texH),
                new Vector2(0.5f, 0.5f), // pivot centre
                _uiStyle.SpritePixelSize);
            Track(sprite);

            return sprite;
        }

        // ?????????????????????????????????????????????????????????????????
        // Per-cell stamping
        // ?????????????????????????????????????????????????????????????????

        /// <summary>
        /// Stamps fill, edges and corners for a single cell. Each cell writes
        /// only to its own region of the texture, so cells can be processed
        /// independently — only the within-cell order (fill ? edges ? corners)
        /// matters for correct alpha blending.
        /// </summary>
        private void StampCell(Vector2Int cell, Vector2Int boundsMin, HashSet<Vector2Int> shapeSet)
        {
            Vector2Int offset = Offset(cell, boundsMin);
            var n = new CellNeighbors(cell, shapeSet);

            if (_fillPixels != null)
                StampFull(offset, _fillPixels);

            if (_edgeHPixels != null || _edgeVPixels != null)
            {
                if (n.ExposedTop) StampEdge(offset, EdgeSide.Top);
                if (n.ExposedBottom) StampEdge(offset, EdgeSide.Bottom);
                if (n.ExposedLeft) StampEdge(offset, EdgeSide.Left);
                if (n.ExposedRight) StampEdge(offset, EdgeSide.Right);
            }

            if (_outerCornerPixels != null)
            {
                if (n.ExposedTop && n.ExposedLeft) StampCorner(offset, _outerCornerPixels, Corner.TopLeft);
                if (n.ExposedTop && n.ExposedRight) StampCorner(offset, _outerCornerPixels, Corner.TopRight);
                if (n.ExposedBottom && n.ExposedLeft) StampCorner(offset, _outerCornerPixels, Corner.BottomLeft);
                if (n.ExposedBottom && n.ExposedRight) StampCorner(offset, _outerCornerPixels, Corner.BottomRight);
            }

            if (_innerCornerPixels != null)
            {
                // Concave notch: two filled orthogonal neighbours whose shared diagonal is missing.
                if (n.Right && n.Top && !n.TopRight) StampCorner(offset, _innerCornerPixels, Corner.TopRight);
                if (n.Left && n.Top && !n.TopLeft) StampCorner(offset, _innerCornerPixels, Corner.TopLeft);
                if (n.Right && n.Bottom && !n.BottomRight) StampCorner(offset, _innerCornerPixels, Corner.BottomRight);
                if (n.Left && n.Bottom && !n.BottomLeft) StampCorner(offset, _innerCornerPixels, Corner.BottomLeft);
            }
        }

        // ?????????????????????????????????????????????????????????????????
        // Stamping primitives
        // ?????????????????????????????????????????????????????????????????

        /// <summary>Blits an entire cellPixelSize×cellPixelSize sprite into the working buffer.</summary>
        private void StampFull(Vector2Int cellOffset, Color[] src)
        {
            int s = _uiStyle.SpritePixelSize;

            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    Color pixel = src[x + y * s];
                    if (pixel.a <= 0f) continue;
                    WritePixel(cellOffset.x + x, cellOffset.y + y, pixel);
                }
        }

        /// <summary>
        /// Stamps only the half of an edge sprite facing the exposed side
        /// (top/bottom half of edgeHorizontal, left/right half of edgeVertical).
        /// </summary>
        private void StampEdge(Vector2Int cellOffset, EdgeSide side)
        {
            int s = _uiStyle.SpritePixelSize;
            int half = s / 2;

            Color[] src = (side == EdgeSide.Top || side == EdgeSide.Bottom)
                ? _edgeHPixels
                : _edgeVPixels;

            if (src == null) return;

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
        /// Stamps one quadrant of a corners sprite (outerCorners or innerCorners)
        /// into the matching corner of the destination cell.
        /// </summary>
        private void StampCorner(Vector2Int cellOffset, Color[] src, Corner corner)
        {
            int s = _uiStyle.SpritePixelSize;
            int half = s / 2;
            bool odd = _uiStyle.SpritePixelSize.Odd();

            int srcX = (corner == Corner.TopRight || corner == Corner.BottomRight) ? half : 0;
            int srcY = (corner == Corner.TopLeft || corner == Corner.TopRight) ? half : 0;

            if (odd && (corner == Corner.BottomRight || corner == Corner.TopRight)) srcX += 1;
            if (odd && (corner == Corner.TopLeft || corner == Corner.TopRight)) srcY += 1;

            // Destination in the cell matches the source quadrant position.
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

        /// <summary>Alpha-composites a pixel into the working buffer (src over dst).</summary>
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

        // ?????????????????????????????????????????????????????????????????
        // Texture / sprite assignment
        // ?????????????????????????????????????????????????????????????????

        private Texture2D CreateTextureFromWorkingBuffer(int texW, int texH)
        {
            var texture = new Texture2D(texW, texH, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(_working);
            texture.Apply();
            return texture;
        }

        private void AssignMainSprite(Sprite sprite)
        {
            if (image == null)
                image = GetComponent<Image>();

            image.sprite = sprite;
            image.alphaHitTestMinimumThreshold = alphaHitThreshold;

            float scale = uiCellSize / _uiStyle.SpritePixelSize;
            GetComponent<RectTransform>().sizeDelta =
                new Vector2(sprite.texture.width, sprite.texture.height) * scale;
        }

        /// <summary>
        /// Builds a white silhouette that follows the tile's actual shape (derived from the
        /// alpha of the just-built <see cref="_working"/> buffer, so it matches corners and
        /// notches exactly), grown outward by <see cref="highlightPaddingPixels"/> so it reads
        /// as a halo rather than an exact duplicate of the tile, then assigns it to
        /// highlightImage. Only used by Build() — GetAllSpriteRotations doesn't need a highlight
        /// per rotation since the highlight is only ever shown for whichever tile is displayed.
        /// </summary>
        private void BuildAndAssignHighlight(Texture2D mainTexture)
        {
            if (highlightImage == null) return;

            int texW = mainTexture.width;
            int texH = mainTexture.height;
            int padding = highlightPaddingPixels;
            int highlightW = texW + padding * 2;
            int highlightH = texH + padding * 2;

            var pixels = new Color[highlightW * highlightH];
            for (int hy = 0; hy < highlightH; hy++)
                for (int hx = 0; hx < highlightW; hx++)
                    if (IsNearTileSilhouette(hx - padding, hy - padding, texW, texH, padding))
                        pixels[hx + hy * highlightW] = Color.white;

            var highlightTexture = new Texture2D(highlightW, highlightH, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            highlightTexture.SetPixels(pixels);
            highlightTexture.Apply();
            Track(highlightTexture);

            var highlightSprite = Sprite.Create(
                highlightTexture,
                new Rect(0, 0, highlightW, highlightH),
                new Vector2(0.5f, 0.5f),
                _uiStyle.SpritePixelSize);
            Track(highlightSprite);

            ReplaceCurrent(ref _currentHighlightTexture, highlightTexture);
            ReplaceCurrent(ref _currentHighlightSprite, highlightSprite);

            highlightImage.sprite = highlightSprite;

            var rt = highlightImage.GetComponent<RectTransform>();
            if (rt != null)
                rt.sizeDelta = new Vector2(highlightW, highlightH) * (uiCellSize / _uiStyle.SpritePixelSize);
        }

        /// <summary>
        /// True if any pixel of the tile's main texture within <paramref name="padding"/>
        /// pixels (Chebyshev distance) of (mx, my) is opaque. (mx, my) may fall outside the
        /// main texture's bounds — those positions are simply treated as empty.
        /// </summary>
        private bool IsNearTileSilhouette(int mx, int my, int texW, int texH, int padding)
        {
            int xMin = Mathf.Max(mx - padding, 0);
            int xMax = Mathf.Min(mx + padding, texW - 1);
            int yMin = Mathf.Max(my - padding, 0);
            int yMax = Mathf.Min(my + padding, texH - 1);

            for (int y = yMin; y <= yMax; y++)
                for (int x = xMin; x <= xMax; x++)
                    if (_working[x + y * texW].a > 0f)
                        return true;

            return false;
        }

        // ?????????????????????????????????????????????????????????????????
        // Pixel caching
        // ?????????????????????????????????????????????????????????????????

        private void CachePixels()
        {
            _fillPixels = GetSpritePixels(_uiStyle.Fill);
            _edgeHPixels = GetSpritePixels(_uiStyle.EdgeHorizontal);
            _edgeVPixels = GetSpritePixels(_uiStyle.EdgeVertical);
            _outerCornerPixels = GetSpritePixels(_uiStyle.OuterCorners);
            _innerCornerPixels = GetSpritePixels(_uiStyle.InnerCorners);
        }

        /// <summary>
        /// Extracts pixels from a sprite's sub-rect in its texture atlas, or null if the
        /// sprite isn't assigned — callers skip stamping whichever passes come back null.
        /// The texture must have Read/Write Enabled in its import settings.
        /// </summary>
        private static Color[] GetSpritePixels(Sprite sprite)
        {
            if (sprite == null) return null;

            Rect rect = sprite.textureRect;
            return sprite.texture.GetPixels(
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height);
        }

        // ?????????????????????????????????????????????????????????????????
        // Bounds & offset helpers
        // ?????????????????????????????????????????????????????????????????

        private Vector2Int Offset(Vector2Int cell, Vector2Int boundsMin)
            => (cell - boundsMin) * _uiStyle.SpritePixelSize;

        private static Vector2Int BoundsMin(List<Vector2Int> shape)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var point in shape)
            {
                if (point.x < minX) minX = point.x;
                if (point.y < minY) minY = point.y;
            }
            return new Vector2Int(minX, minY);
        }

        private static Vector2Int BoundsMax(List<Vector2Int> shape)
        {
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var point in shape)
            {
                if (point.x > maxX) maxX = point.x;
                if (point.y > maxY) maxY = point.y;
            }
            return new Vector2Int(maxX, maxY);
        }

        // ?????????????????????????????????????????????????????????????????
        // Resource lifetime
        // ?????????????????????????????????????????????????????????????????

        /// <summary>Registers a Texture2D or Sprite this component created, so OnDestroy cleans it up.</summary>
        private void Track(Object resource) => _ownedResources.Add(resource);

        /// <summary>
        /// Destroys and untracks the current value of <paramref name="current"/> (if any),
        /// then stores <paramref name="replacement"/> in its place. Used for the "currently
        /// displayed" texture/sprite fields, which are replaced — not merely accumulated —
        /// each time Build() runs.
        /// </summary>
        private void ReplaceCurrent<T>(ref T current, T replacement) where T : Object
        {
            if (current != null)
            {
                _ownedResources.Remove(current);
                Destroy(current);
            }
            current = replacement;
        }

        private void OnDestroy()
        {
            foreach (Object resource in _ownedResources)
                if (resource != null) Destroy(resource);
            _ownedResources.Clear();
            _rotationSprites = null;
        }
    }
}