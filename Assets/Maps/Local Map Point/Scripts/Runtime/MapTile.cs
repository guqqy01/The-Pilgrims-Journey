using LocalMap.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMap
{
    public class MapTile : MonoBehaviour
    {
        public Vector2Int Position { private set; get; }

        public SpriteRenderer graphic;

        public SpriteRenderer highlight;

        public SpriteRenderer highlightLeft;
        public SpriteRenderer highlightUp;
        public SpriteRenderer highlightRight;
        public SpriteRenderer highlightDown;

        public SpriteRenderer highlightLeftUpCorner;
        public SpriteRenderer highlightRightUpCorner;
        public SpriteRenderer highlightLeftDownCorner;
        public SpriteRenderer highlightRightDownCorner;

        private bool _walkable;
        public bool IsFullyWalkable => _walkable && _blockedRegions.Count == 0;
        private readonly List<AutoTileWalkability> _autoTileWalkability = new List<AutoTileWalkability>();
        public List<AutoTileWalkability> AutoTileWalkability => _autoTileWalkability;
        private readonly List<Rect> _blockedRegions = new List<Rect>();
        private struct AutoTileTypeRegion
        {
            public TileType OverType;
            public AutoTileWalkability Bounds;
            public int DrawOrder;
        }

        private readonly List<AutoTileTypeRegion> _autoTileTypeRegions = new List<AutoTileTypeRegion>();

        public bool HasAutoTileWalkability => _autoTileWalkability.Count > 0;

        public TileTypeAndVisual TileTypeAndVisual;
        public TileType TileType => TileTypeAndVisual.TileType;
        public TileVisualType TileVisualType => TileTypeAndVisual.TileVisualType;
        private int currentFrameIndex = 0;
        private float timeToChangeSprite;

        private bool highlighted;
        public bool Highlighted
        {
            get
            {
                return highlighted;
            }
            set
            {
                highlighted = value;

                highlight.gameObject.SetActive(highlighted);
                highlight.color = Color.white.SetAlpha(0.2f);

                highlightLeft.gameObject.SetActive(highlighted);
                highlightLeft.color = Color.white;

                highlightUp.gameObject.SetActive(highlighted);
                highlightUp.color = Color.white;

                highlightRight.gameObject.SetActive(highlighted);
                highlightRight.color = Color.white;

                highlightDown.gameObject.SetActive(highlighted);
                highlightDown.color = Color.white;

                highlightLeftUpCorner.gameObject.SetActive(highlighted);
                highlightLeftUpCorner.color = Color.white;

                highlightRightUpCorner.gameObject.SetActive(highlighted);
                highlightRightUpCorner.color = Color.white;

                highlightLeftDownCorner.gameObject.SetActive(highlighted);
                highlightLeftDownCorner.color = Color.white;

                highlightRightDownCorner.gameObject.SetActive(highlighted);
                highlightRightDownCorner.color = Color.white;
            }
        }
        public void AddBlockedRegion(Rect relativeRect)
        {
            _blockedRegions.Add(relativeRect);
        }

        public void RemoveBlockedRegion(Rect relativeRect)
        {
            _blockedRegions.Remove(relativeRect);
        }

        public bool HasBlockedRegions => _blockedRegions.Count > 0;
        public void Init(TileTypeAndVisual tileTypeAndSprite, Vector2Int position, bool walkable)
        {
            TileTypeAndVisual = tileTypeAndSprite;

            switch (tileTypeAndSprite.TileVisualType)
            {
                case TileVisualType.SingleSprite:
                    graphic.sprite = tileTypeAndSprite.Sprite;
                    break;
                case TileVisualType.SpriteAnimation:
                    if(tileTypeAndSprite.SpriteAnimation.Frames != null && tileTypeAndSprite.SpriteAnimation.Frames.Length > 0)
                        graphic.sprite = tileTypeAndSprite.SpriteAnimation.Frames[0];
                    break;
            }
            timeToChangeSprite = Time.time;

            Highlighted = false;
            Position = position;
            _walkable = walkable;
        }

        private void Update()
        {
            if (TileVisualType != TileVisualType.SpriteAnimation)
                return;
            
            if (timeToChangeSprite > Time.time)
                return;

            if (TileTypeAndVisual.SpriteAnimation.Frames == null || TileTypeAndVisual.SpriteAnimation.Frames.Length == 0)
                return;

            currentFrameIndex = (currentFrameIndex + 1) % TileTypeAndVisual.SpriteAnimation.Frames.Length;

            graphic.sprite = (Sprite)TileTypeAndVisual.SpriteAnimation.Frames.GetRandom();

            timeToChangeSprite = Time.time + 1f / TileTypeAndVisual.SpriteAnimation.FramesPerSecond;
        }

        internal bool IsPositionWalkable(Vector2 position)
        {
            if (position.V2ToV2Int() != Position)
                return false;

            Vector2 relativePosition = position - (Vector2)Position + Vector2.one / 2f;

            foreach (Rect blocked in _blockedRegions)
            {
                if (blocked.Contains(relativePosition))
                    return false;
            }

            if (_walkable)
                return true;

            foreach (AutoTileWalkability autoTileWalkability in _autoTileWalkability)
            {
                if (autoTileWalkability.Contains(relativePosition))
                    return true;
            }

            return false;
        }

        public TileType? GetTileTypeAtPosition(Vector2 position)
        {
            if (position.V2ToV2Int() != Position)
                return null;

            Vector2 relativePosition = position - (Vector2)Position + Vector2.one / 2f;

            TileType? topMostOverType = null;
            int topMostDrawOrder = int.MinValue;

            foreach (AutoTileTypeRegion region in _autoTileTypeRegions)
            {
                if (region.DrawOrder <= topMostDrawOrder)
                    continue;

                if (region.Bounds.Contains(relativePosition))
                {
                    topMostDrawOrder = region.DrawOrder;
                    topMostOverType = region.OverType;
                }
            }

            return topMostOverType ?? TileType;
        }

        internal bool TryGetNearestWalkablePosition(Vector2 searchOriginPosition, out Vector2 walkablePos)
        {
            if (_walkable)
            {
                float xPos = Mathf.Clamp(searchOriginPosition.x, Position.x - 0.5f, Position.x + 0.5f);
                float yPos = Mathf.Clamp(searchOriginPosition.y, Position.y - 0.5f, Position.y + 0.5f);
                walkablePos = new Vector2(xPos, yPos);
                return true;
            }

            Vector2 relativePosition = searchOriginPosition - (Vector2)Position + Vector2.one / 2f;

            if(_autoTileWalkability.Count == 0)
            {
                walkablePos = searchOriginPosition;
                return false;
            }

            walkablePos = searchOriginPosition;
            float minDistanceToClosestWalkable = float.MaxValue;

            foreach (AutoTileWalkability autoTileWalkability in _autoTileWalkability)
            {
                Vector2 relativeWalkablePos = autoTileWalkability.GetClosestPoint(relativePosition);
                Vector2 worldWalkablePos = (Vector2)Position + relativeWalkablePos - Vector2.one / 2f;
                float distanceToClosestWalkable = Vector2.Distance(searchOriginPosition, worldWalkablePos);

                if (distanceToClosestWalkable < minDistanceToClosestWalkable)
                {
                    walkablePos = worldWalkablePos;
                    minDistanceToClosestWalkable = distanceToClosestWalkable;
                }
            }

            return true;
        }

        public void HighlightWithColor(Color color, Grid3x3<bool> boolGrid)
        {
            highlighted = true;

            if (highlight == null)
                return;

            highlight.gameObject.SetActive(highlighted);
            highlight.color = color.SetAlpha(0.2f);

            highlightLeft.gameObject.SetActive(boolGrid[0, 1]);
            highlightLeft.color = color;

            highlightUp.gameObject.SetActive(boolGrid[1, 2]);
            highlightUp.color = color;

            highlightRight.gameObject.SetActive(boolGrid[2, 1]);
            highlightRight.color = color;

            highlightDown.gameObject.SetActive(boolGrid[1, 0]);
            highlightDown.color = color;

            highlightLeftUpCorner.gameObject.SetActive(boolGrid[0, 2]);
            highlightLeftUpCorner.color = color;

            highlightRightUpCorner.gameObject.SetActive(boolGrid[2, 2]);
            highlightRightUpCorner.color = color;

            highlightLeftDownCorner.gameObject.SetActive(boolGrid[0, 0]);
            highlightLeftDownCorner.color = color;

            highlightRightDownCorner.gameObject.SetActive(boolGrid[2, 0]);
            highlightRightDownCorner.color = color;
        }


        public void SetupAutoTiles(AutoTileSettings autoTilesSettings, Grid3x3<TileType> tileGrid)
        {
            if (autoTilesSettings == null)
                return;

            TileType underType = TileTypeAndVisual.TileType;

            HashSet<TileType> neighbourTypes = new HashSet<TileType>()
            {
                tileGrid.TopLeft,
                tileGrid.TopCenter,
                tileGrid.TopRight,
                tileGrid.CenterLeft,
                tileGrid.CenterRight,
                tileGrid.BottomLeft,
                tileGrid.BottomCenter,
                tileGrid.BottomRight,
            };
            neighbourTypes.Remove(underType);
            neighbourTypes.Remove(TileType.Nothing);

            foreach (TileType overType in neighbourTypes)
            {
                AutoTiles autoTile = autoTilesSettings.GetAutoTile(underType, overType);
                if (autoTile == null)
                    continue;

                Grid3x3<bool> boolGrid = BuildBoolGrid(tileGrid, overType);

                int sortingOrder = 2 * autoTilesSettings.drawOrderForType.IndexOf(overType);

                if (AutoTiles.StraightTopCenter(boolGrid)) SpawnAutoTile(autoTile.TopCenterAutoTile, sortingOrder, overType);
                if (AutoTiles.StraightCenterLeft(boolGrid)) SpawnAutoTile(autoTile.CenterLeftAutoTile, sortingOrder, overType);
                if (AutoTiles.StraightCenterRight(boolGrid)) SpawnAutoTile(autoTile.CenterRightAutoTile, sortingOrder, overType);
                if (AutoTiles.StraightBottomCenter(boolGrid)) SpawnAutoTile(autoTile.BottomCenterAutoTile, sortingOrder, overType);

                if (AutoTiles.SmallCornerTopLeft(boolGrid)) SpawnAutoTile(autoTile.TopLeftAutoTile, sortingOrder, overType);
                if (AutoTiles.SmallCornerTopRight(boolGrid)) SpawnAutoTile(autoTile.TopRightAutoTile, sortingOrder, overType);
                if (AutoTiles.SmallCornerBottomLeft(boolGrid)) SpawnAutoTile(autoTile.BottomLeftAutoTile, sortingOrder, overType);
                if (AutoTiles.SmallCornerBottomRight(boolGrid)) SpawnAutoTile(autoTile.BottomRightAutoTile, sortingOrder, overType);

                if (AutoTiles.BigCornerTopLeft(boolGrid)) SpawnAutoTile(autoTile.TopAndLeftAutoTile, sortingOrder, overType);
                if (AutoTiles.BigCornerTopRight(boolGrid)) SpawnAutoTile(autoTile.TopAndRightAutoTile, sortingOrder, overType);
                if (AutoTiles.BigCornerBottomLeft(boolGrid)) SpawnAutoTile(autoTile.BottomAndLeftAutoTile, sortingOrder, overType);
                if (AutoTiles.BigCornerBottomRight(boolGrid)) SpawnAutoTile(autoTile.BottomAndRightAutoTile, sortingOrder, overType);
            }
        }

        private static Grid3x3<bool> BuildBoolGrid(Grid3x3<TileType> tileGrid, TileType overType)
        {
            return new Grid3x3<bool>()
            {
                TopLeft = tileGrid.TopLeft == overType,
                TopCenter = tileGrid.TopCenter == overType,
                TopRight = tileGrid.TopRight == overType,
                CenterLeft = tileGrid.CenterLeft == overType,
                CenterRight = tileGrid.CenterRight == overType,
                BottomLeft = tileGrid.BottomLeft == overType,
                BottomCenter = tileGrid.BottomCenter == overType,
                BottomRight = tileGrid.BottomRight == overType,
            };
        }

        public void SpawnAutoTile(AutoTileSpriteAndWalkability autoTile, int sortingOrder, TileType? overType = null)
        {
            if (autoTile.BaseSprite != null)
            {
                SpriteRenderer normalTile = Instantiate(graphic, transform.position, Quaternion.identity, transform);
                normalTile.sprite = autoTile.BaseSprite;
                normalTile.sortingOrder = sortingOrder;
            }

            if (autoTile.OutlineSprite != null)
            {
                SpriteRenderer outlineTile = Instantiate(graphic, transform.position, Quaternion.identity, transform);
                outlineTile.sprite = autoTile.OutlineSprite;
                outlineTile.sortingOrder = sortingOrder + 1;
            }

            _autoTileWalkability.AddRange(autoTile.walkabilityBounds);

            if (overType.HasValue)
            {
                foreach (AutoTileWalkability bounds in autoTile.walkabilityBounds)
                {
                    _autoTileTypeRegions.Add(new AutoTileTypeRegion
                    {
                        OverType = overType.Value,
                        Bounds = bounds,
                        DrawOrder = sortingOrder
                    });
                }
            }
        }
    }

    public enum TileVisualType
    {
        SingleSprite,
        SpriteAnimation,
    }

    [Serializable]
    public class TileTypeAndVisual
    {
        public TileType TileType;

        public TileVisualType TileVisualType;
        public Sprite Sprite;
        public SpriteAnimation SpriteAnimation;
        
        public TileTypeAndVisual(Sprite sprite, TileType tileType)
        {
            this.Sprite = sprite;
            this.TileType = tileType;
        }
    }

    [Serializable]
    public class SpriteAnimation
    {
        public Sprite[] Frames;
        public float FramesPerSecond;
    }
}
