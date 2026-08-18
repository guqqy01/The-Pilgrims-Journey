using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Tiles
{
    /// <summary>
    /// Builds a Texture2D for any tile shape and assigns it to a RawImage.
    /// Call Build(shape, uiStyle) once when the tile is created or its shape changes,
    /// or GetAllSpriteRotations(shape, uiStyle) to pre-build one sprite per 90° rotation
    /// and ChangeSprite(rotation) to switch between them instantly afterwards.
    ///
    /// Rotation does NOT change the tile's shape/geometry — the shape is always laid out
    /// and stamped exactly as given. What changes per rotation is which art gets drawn at
    /// each exposed side/corner of a cell:
    ///   - Corners (outerCorners/innerCorners) rotate as a whole stamp — a symmetric 9-slice
    ///     border means the same physical key (TopLeft, etc.) stays valid on the rotated stamp.
    ///   - Edges are trickier: edgeHorizontal only ever supplies Top/Bottom art and edgeVertical
    ///     only ever supplies Left/Right art. At 90°/270°, the physical Top position may need
    ///     to be fed by edgeVertical's Left piece (or similar) — see StampEdge for the mapping.
    ///     The extracted piece is rotated on its own before being placed, so its content reads
    ///     correctly once repositioned.
    ///
    /// Each of the sprites below is optional — any that aren't assigned on
    /// TileUIStyle are simply skipped, so e.g. a style with no OuterCorners
    /// sprite will render without outer corner pieces. Every sprite that is
    /// assigned must be cellPixelSize × cellPixelSize, and its Sprite.border
    /// must be set to the half-cell 9-slice border so Stamp can slice out the
    /// edge halves / corner quadrants:
    ///
    ///   fill            — solid interior, no border pixels, no directional content
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
    /// Compositing is done via <see cref="Stamp"/>: each fill/edge/corner sprite is
    /// cached as a Stamp, edges/corners are pre-sliced into halves/quadrants via
    /// Stamp's built-in 9-slice SubStamps, and each piece is blitted onto the
    /// working canvas with Stamp.StampFull.
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

        private static readonly Stamp.ClockwiseRotations[] RotationAngles =
        {
            Stamp.ClockwiseRotations.None,
            Stamp.ClockwiseRotations.Deg90,
            Stamp.ClockwiseRotations.Deg180,
            Stamp.ClockwiseRotations.Deg270,
        };

        // Physical edge positions, in clockwise order around the cell. TopCenter/
        // BottomCenter are natively supplied by edgeHorizontal; CenterLeft/CenterRight
        // by edgeVertical — together they form one 4-slot ring. See StampEdge.
        private static readonly Stamp.BaseSubStamps[] EdgeCycle =
        {
            Stamp.BaseSubStamps.TopCenter,
            Stamp.BaseSubStamps.CenterRight,
            Stamp.BaseSubStamps.BottomCenter,
            Stamp.BaseSubStamps.CenterLeft,
        };

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
        // All entries share the same shape/dimensions — only decoration differs.
        private Dictionary<Stamp.ClockwiseRotations, Sprite> _rotationSprites;

        // Cached per-piece Stamps, rebuilt whenever CacheStamps() runs for a
        // (possibly new) _uiStyle. Only the corner stamps are built with
        // createRotationStamps: true (see StampEdge for why edges are handled
        // differently). Edge/corner stamps also carry pre-sliced SubStamps
        // (halves/quadrants) via their sprite's own border.
        private Stamp _fillStamp;
        private Stamp _edgeHStamp;
        private Stamp _edgeVStamp;
        private Stamp _outerCornerStamp;
        private Stamp _innerCornerStamp;

        private TileUIStyle _uiStyle;

        // Working buffer — all stamping writes here, flushed once at the end.
        // Reused across sequential BuildSprite calls; each call fully
        // completes before the next starts, so this is safe.
        private Color[] _working;
        private int _texW;

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

        public Sprite Build(List<Vector2Int> shape, TileUIStyle uiStyle, Stamp.ClockwiseRotations rotation = Stamp.ClockwiseRotations.None)
        {
            _uiStyle = uiStyle;
            CacheStamps();

            Sprite sprite = BuildSprite(shape, rotation, out Texture2D texture);
            if (sprite == null) return null;

            ReplaceCurrent(ref _currentTexture, texture);
            ReplaceCurrent(ref _currentSprite, sprite);
            AssignMainSprite(sprite);

            BuildAndAssignHighlight(texture);

            return sprite;
        }

        public Dictionary<Stamp.ClockwiseRotations, Sprite> GetAllSpriteRotations(List<Vector2Int> shape, TileUIStyle uiStyle)
        {
            _uiStyle = uiStyle;
            CacheStamps();

            ReleaseCachedRotations();

            var rotations = new Dictionary<Stamp.ClockwiseRotations, Sprite>();
            foreach (Stamp.ClockwiseRotations angle in RotationAngles)
            {
                Sprite sprite = BuildSprite(shape, angle, out _);
                if (sprite != null)
                    rotations[angle] = sprite;
            }
            _rotationSprites = rotations;
            return rotations;
        }

        public void ChangeSprite(Stamp.ClockwiseRotations rotation)
        {
            if (_rotationSprites == null || !_rotationSprites.TryGetValue(rotation, out Sprite sprite) || sprite == null)
            {
                Debug.LogWarning($"[TileShapeTextureBuilder] {name}: no cached sprite for {rotation}° — call GetAllSpriteRotations first.");
                return;
            }
            AssignMainSprite(sprite);
        }

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

        private Sprite BuildSprite(List<Vector2Int> shape, Stamp.ClockwiseRotations rotation, out Texture2D texture)
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
                StampCell(cell, boundsMin, shapeSet, rotation);

            texture = new Stamp(_working, texW).ToTexture2D();
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

        private void StampCell(Vector2Int cell, Vector2Int boundsMin, HashSet<Vector2Int> shapeSet, Stamp.ClockwiseRotations rotation)
        {
            Vector2Int offset = Offset(cell, boundsMin);
            var n = new CellNeighbors(cell, shapeSet);

            _fillStamp?.StampFull(_working, _texW, offset);

            if (n.ExposedTop) StampEdge(Stamp.BaseSubStamps.TopCenter, offset, rotation);
            if (n.ExposedBottom) StampEdge(Stamp.BaseSubStamps.BottomCenter, offset, rotation);
            if (n.ExposedLeft) StampEdge(Stamp.BaseSubStamps.CenterLeft, offset, rotation);
            if (n.ExposedRight) StampEdge(Stamp.BaseSubStamps.CenterRight, offset, rotation);

            Stamp outerCorner = GetRotatedStamp(_outerCornerStamp, rotation);
            Stamp innerCorner = GetRotatedStamp(_innerCornerStamp, rotation);

            if (n.ExposedTop && n.ExposedLeft) StampSub(outerCorner, Stamp.BaseSubStamps.TopLeft, offset);
            if (n.ExposedTop && n.ExposedRight) StampSub(outerCorner, Stamp.BaseSubStamps.TopRight, offset);
            if (n.ExposedBottom && n.ExposedLeft) StampSub(outerCorner, Stamp.BaseSubStamps.BottomLeft, offset);
            if (n.ExposedBottom && n.ExposedRight) StampSub(outerCorner, Stamp.BaseSubStamps.BottomRight, offset);

            // Concave notch: two filled orthogonal neighbours whose shared diagonal is missing.
            if (n.Right && n.Top && !n.TopRight) StampSub(innerCorner, Stamp.BaseSubStamps.TopRight, offset);
            if (n.Left && n.Top && !n.TopLeft) StampSub(innerCorner, Stamp.BaseSubStamps.TopLeft, offset);
            if (n.Right && n.Bottom && !n.BottomRight) StampSub(innerCorner, Stamp.BaseSubStamps.BottomRight, offset);
            if (n.Left && n.Bottom && !n.BottomLeft) StampSub(innerCorner, Stamp.BaseSubStamps.BottomLeft, offset);
        }

        /// <summary>
        /// Returns the variant of <paramref name="source"/> matching <paramref name="rotation"/>:
        /// the stamp itself for None, or its cached RotatedStamps entry for 90/180/270. Used for
        /// corners only — a symmetric 9-slice border means the same physical key stays valid on
        /// the rotated stamp, so no cross-piece remapping is needed (unlike edges — see StampEdge).
        /// </summary>
        private static Stamp GetRotatedStamp(Stamp source, Stamp.ClockwiseRotations rotation)
        {
            if (source == null || rotation == Stamp.ClockwiseRotations.None) return source;

            if (source.RotatedStamps == null ||
                !source.RotatedStamps.TryGetValue(rotation, out Stamp rotated))
            {
                Debug.LogWarning($"[TileShapeTextureBuilder] Stamp has no cached {rotation}° rotation — was it built with createRotationStamps: true?");
                return source;
            }

            return rotated;
        }

        /// <summary>
        /// Stamps whichever edge art should appear at the physical position <paramref name="physicalKey"/>
        /// once <paramref name="rotation"/> is taken into account. Because edgeHorizontal only supplies
        /// Top/Bottom art and edgeVertical only supplies Left/Right art, a 90°/270° rotation can require
        /// pulling from the OTHER sprite (e.g. rotated 90°, the physical Top position is fed by
        /// edgeVertical's Left piece) — RotateEdgeKeyBackward finds which native slot supplies it, and
        /// the extracted piece is then rotated in place so its content reads correctly once repositioned.
        /// No-ops if the required native piece isn't assigned in this style.
        /// </summary>
        private void StampEdge(Stamp.BaseSubStamps physicalKey, Vector2Int cellOffset, Stamp.ClockwiseRotations rotation)
        {
            Stamp.BaseSubStamps sourceKey = RotateEdgeKeyBackward(physicalKey, rotation);
            Stamp nativeStamp = NativeEdgeStamp(sourceKey);

            if (nativeStamp?.SubStamps == null) return;
            if (!nativeStamp.SubStamps.TryGetValue(sourceKey, out Stamp piece)) return;

            Stamp rotatedPiece = piece.Rotated(rotation);
            Vector2Int local = EdgeLocalOffset(physicalKey, rotatedPiece);
            rotatedPiece.StampFull(_working, _texW, cellOffset + local);
        }

        /// <summary>
        /// Which native (unrotated) edge slot supplies the art that should appear at
        /// <paramref name="physicalKey"/> once the tile is virtually rotated by
        /// <paramref name="rotation"/>. Walks EdgeCycle backwards by the rotation amount —
        /// e.g. at 90°, physical Top is fed by the slot one step counter-clockwise of Top,
        /// i.e. Left.
        /// </summary>
        private static Stamp.BaseSubStamps RotateEdgeKeyBackward(Stamp.BaseSubStamps physicalKey, Stamp.ClockwiseRotations rotation)
        {
            int steps = (int)rotation / 90;
            int idx = Array.IndexOf(EdgeCycle, physicalKey);
            int sourceIdx = ((idx - steps) % EdgeCycle.Length + EdgeCycle.Length) % EdgeCycle.Length;
            return EdgeCycle[sourceIdx];
        }

        /// <summary>Which style sprite (edgeHorizontal vs edgeVertical) natively supplies a given edge key.</summary>
        private Stamp NativeEdgeStamp(Stamp.BaseSubStamps key)
        {
            return key == Stamp.BaseSubStamps.TopCenter || key == Stamp.BaseSubStamps.BottomCenter
                ? _edgeHStamp
                : _edgeVStamp;
        }

        /// <summary>
        /// Where a rotated edge piece sits within its cell: flush against the physical side
        /// it's being drawn for (Top flush to the top, Right flush to the right, etc.), sized
        /// by the piece's own (post-rotation) width/height.
        /// </summary>
        private Vector2Int EdgeLocalOffset(Stamp.BaseSubStamps physicalKey, Stamp piece)
        {
            int cell = _uiStyle.SpritePixelSize;
            switch (physicalKey)
            {
                case Stamp.BaseSubStamps.TopCenter: return new Vector2Int(0, cell - piece.Height);
                case Stamp.BaseSubStamps.BottomCenter: return Vector2Int.zero;
                case Stamp.BaseSubStamps.CenterRight: return new Vector2Int(cell - piece.Width, 0);
                case Stamp.BaseSubStamps.CenterLeft: return Vector2Int.zero;
                default: return Vector2Int.zero;
            }
        }

        /// <summary>
        /// Looks up <paramref name="key"/> in <paramref name="source"/>'s pre-sliced
        /// SubStamps and blits it into the working buffer at the matching position
        /// within the cell. No-ops if <paramref name="source"/> is null or has no
        /// sub-stamp for that key. Used for corners only — see StampEdge for edges.
        /// </summary>
        private void StampSub(Stamp source, Stamp.BaseSubStamps key, Vector2Int cellOffset)
        {
            if (source?.SubStamps == null) return;
            if (!source.SubStamps.TryGetValue(key, out Stamp sub)) return;

            Vector2Int local = SubStampLocalOffset(source, sub, key);
            sub.StampFull(_working, _texW, cellOffset + local);
        }

        private static Vector2Int SubStampLocalOffset(Stamp parent, Stamp sub, Stamp.BaseSubStamps key)
        {
            const Stamp.BaseSubStamps rightKeys = Stamp.BaseSubStamps.TopRight
                | Stamp.BaseSubStamps.CenterRight | Stamp.BaseSubStamps.BottomRight;
            const Stamp.BaseSubStamps topKeys = Stamp.BaseSubStamps.TopLeft
                | Stamp.BaseSubStamps.TopCenter | Stamp.BaseSubStamps.TopRight;

            int x = (key & rightKeys) != 0 ? parent.Width - sub.Width : 0;
            int y = (key & topKeys) != 0 ? parent.Height - sub.Height : 0;
            return new Vector2Int(x, y);
        }

        // ?????????????????????????????????????????????????????????????????
        // Texture / sprite assignment
        // ?????????????????????????????????????????????????????????????????

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

            var highlightTexture = new Stamp(pixels, highlightW).ToTexture2D();
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
        // Stamp caching
        // ?????????????????????????????????????????????????????????????????

        /// <summary>
        /// Builds/refreshes the Stamps used by StampCell. Only the corner stamps need
        /// createRotationStamps: true (see GetRotatedStamp) — edges are rotated per-piece
        /// on demand in StampEdge instead, so their base stamps don't need pre-built rotations.
        /// </summary>
        private void CacheStamps()
        {
            _fillStamp = BuildStamp(_uiStyle.Fill, createRotationStamps: false);
            _edgeHStamp = BuildStamp(_uiStyle.EdgeHorizontal, createRotationStamps: false);
            _edgeVStamp = BuildStamp(_uiStyle.EdgeVertical, createRotationStamps: false);
            _outerCornerStamp = BuildStamp(_uiStyle.OuterCorners, createRotationStamps: true);
            _innerCornerStamp = BuildStamp(_uiStyle.InnerCorners, createRotationStamps: true);
        }

        private static Stamp BuildStamp(Sprite sprite, bool createRotationStamps)
        {
            return sprite == null ? null : new Stamp(sprite, createRotationStamps);
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

        private void Track(Object resource) => _ownedResources.Add(resource);

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