using LocalMap.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

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
        public bool FullyWalkable => _walkable;
        private readonly List<AutoTileWalkability> _autoTileWalkability = new List<AutoTileWalkability>();
        public List<AutoTileWalkability> AutoTileWalkability => _autoTileWalkability;

        public bool HasAutoTileWalkability => _autoTileWalkability.Count > 0;

        public TileType TileType;

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

        public void Init(TileTypeAndSprite tileTypeAndSprite, Vector2Int position, bool walkable)
        {
            graphic.sprite = tileTypeAndSprite.Sprite;
            Highlighted = false;
            Position = position;
            _walkable = walkable;
            TileType = tileTypeAndSprite.TileType;
        }

        internal bool IsPositionWalkable(Vector2 position)
        {
            //Position must be within tile bounds to be walkable
            if (position.V2ToV2Int() != Position)
                return false;

            if (_walkable)
                return true;

            foreach (AutoTileWalkability autoTileWalkability in _autoTileWalkability)
            {
                Vector2 relativePosition = position - (Vector2)Position - Vector2.one / 2f;
                if (autoTileWalkability.Contains(relativePosition))
                {
                    Debug.Log($"Tile in Position: {Position} is walkable from autotile");
                    return true;
                }
            }

            return false;
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

            TileType underType = TileType;

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

                if (AutoTiles.StraightTopCenter(boolGrid)) SpawnAutoTile(autoTile.TopCenterAutoTile, sortingOrder);
                if (AutoTiles.StraightCenterLeft(boolGrid)) SpawnAutoTile(autoTile.CenterLeftAutoTile, sortingOrder);
                if (AutoTiles.StraightCenterRight(boolGrid)) SpawnAutoTile(autoTile.CenterRightAutoTile, sortingOrder);
                if (AutoTiles.StraightBottomCenter(boolGrid)) SpawnAutoTile(autoTile.BottomCenterAutoTile, sortingOrder);

                if (AutoTiles.SmallCornerTopLeft(boolGrid)) SpawnAutoTile(autoTile.TopLeftAutoTile, sortingOrder);
                if (AutoTiles.SmallCornerTopRight(boolGrid)) SpawnAutoTile(autoTile.TopRightAutoTile, sortingOrder);
                if (AutoTiles.SmallCornerBottomLeft(boolGrid)) SpawnAutoTile(autoTile.BottomLeftAutoTile, sortingOrder);
                if (AutoTiles.SmallCornerBottomRight(boolGrid)) SpawnAutoTile(autoTile.BottomRightAutoTile, sortingOrder);

                if (AutoTiles.BigCornerTopLeft(boolGrid)) SpawnAutoTile(autoTile.TopAndLeftAutoTile, sortingOrder);
                if (AutoTiles.BigCornerTopRight(boolGrid)) SpawnAutoTile(autoTile.TopAndRightAutoTile, sortingOrder);
                if (AutoTiles.BigCornerBottomLeft(boolGrid)) SpawnAutoTile(autoTile.BottomAndLeftAutoTile, sortingOrder);
                if (AutoTiles.BigCornerBottomRight(boolGrid)) SpawnAutoTile(autoTile.BottomAndRightAutoTile, sortingOrder);
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

        public void SpawnAutoTile(AutoTileSpriteAndWalkability autoTile, int sortingOrder)
        {
            if(autoTile.BaseSprite != null)
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
        }
    }

    [Serializable]
    public class TileTypeAndSprite
    {
        public Sprite Sprite;
        public TileType TileType;

        public TileTypeAndSprite(Sprite sprite, TileType tileType)
        {
            this.Sprite = sprite;
            this.TileType = tileType;
        }
    }
}
