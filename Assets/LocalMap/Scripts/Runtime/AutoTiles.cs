using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace LocalMap
{
    [Serializable]
    public class AutoTiles
    {
        public TileType OverTileType;
        public TileType UnderTileType;

        public Sprite OverBase;
        public Sprite UnderBase;

        public AutoTileSpriteAndWalkability TopCenterAutoTile;
        public AutoTileSpriteAndWalkability CenterLeftAutoTile;
        public AutoTileSpriteAndWalkability CenterRightAutoTile;
        public AutoTileSpriteAndWalkability BottomCenterAutoTile;

        public AutoTileSpriteAndWalkability TopRightAutoTile;
        public AutoTileSpriteAndWalkability TopLeftAutoTile;
        public AutoTileSpriteAndWalkability BottomLeftAutoTile;
        public AutoTileSpriteAndWalkability BottomRightAutoTile;

        public AutoTileSpriteAndWalkability TopAndLeftAutoTile;
        public AutoTileSpriteAndWalkability TopAndRightAutoTile;
        public AutoTileSpriteAndWalkability BottomAndLeftAutoTile;
        public AutoTileSpriteAndWalkability BottomAndRightAutoTile;

        public List<Sprite> GetNormalSprites(Grid3x3<bool> boolGrid)
        {
            List<Sprite> returnList = new List<Sprite>();

            if (StraightBottomCenter(boolGrid))
            {
                returnList.Add(BottomCenterAutoTile.BaseSprite);
            }

            if (StraightCenterLeft(boolGrid))
            {
                returnList.Add(CenterLeftAutoTile.BaseSprite);
            }

            if (StraightCenterRight(boolGrid))
            {
                returnList.Add(CenterRightAutoTile.BaseSprite);
            }

            if (StraightTopCenter(boolGrid))
            {
                returnList.Add(TopCenterAutoTile.BaseSprite);
            }

            if (SmallCornerBottomLeft(boolGrid))
            {
                returnList.Add(BottomLeftAutoTile.BaseSprite);
            }

            if (SmallCornerBottomRight(boolGrid))
            {
                returnList.Add(BottomRightAutoTile.BaseSprite);
            }

            if (SmallCornerTopLeft(boolGrid))
            {
                returnList.Add(TopLeftAutoTile.BaseSprite);
            }

            if (SmallCornerTopRight(boolGrid))
            {
                returnList.Add(TopRightAutoTile.BaseSprite);
            }

            if (BigCornerBottomLeft(boolGrid))
            {
                returnList.Add(BottomAndLeftAutoTile.BaseSprite);
            }

            if (BigCornerBottomRight(boolGrid))
            {
                returnList.Add(BottomAndRightAutoTile.BaseSprite);
            }

            if (BigCornerTopLeft(boolGrid))
            {
                returnList.Add(TopAndLeftAutoTile.BaseSprite);
            }

            if (BigCornerTopRight(boolGrid))
            {
                returnList.Add(TopAndRightAutoTile.BaseSprite);
            }

            return returnList;
        }

        public List<Sprite> GetOutlineSprites(Grid3x3<bool> boolGrid)
        {
            List<Sprite> returnList = new List<Sprite>();

            if (StraightBottomCenter(boolGrid))
            {
                returnList.Add(BottomCenterAutoTile.OutlineSprite);
            }

            if (StraightCenterLeft(boolGrid))
            {
                returnList.Add(CenterLeftAutoTile.OutlineSprite);
            }

            if (StraightCenterRight(boolGrid))
            {
                returnList.Add(CenterRightAutoTile.OutlineSprite);
            }

            if (StraightTopCenter(boolGrid))
            {
                returnList.Add(TopCenterAutoTile.OutlineSprite);
            }

            if (SmallCornerBottomLeft(boolGrid))
            {
                returnList.Add(BottomLeftAutoTile.OutlineSprite);
            }

            if (SmallCornerBottomRight(boolGrid))
            {
                returnList.Add(BottomRightAutoTile.OutlineSprite);
            }

            if (SmallCornerTopLeft(boolGrid))
            {
                returnList.Add(TopLeftAutoTile.OutlineSprite);
            }

            if (SmallCornerTopRight(boolGrid))
            {
                returnList.Add(TopRightAutoTile.OutlineSprite);
            }

            if (BigCornerBottomLeft(boolGrid))
            {
                returnList.Add(BottomAndLeftAutoTile.OutlineSprite);
            }

            if (BigCornerBottomRight(boolGrid))
            {
                returnList.Add(BottomAndRightAutoTile.OutlineSprite);
            }

            if (BigCornerTopLeft(boolGrid))
            {
                returnList.Add(TopAndLeftAutoTile.OutlineSprite);
            }

            if (BigCornerTopRight(boolGrid))
            {
                returnList.Add(TopAndRightAutoTile.OutlineSprite);
            }

            return returnList;
        }

        public static bool StraightTopCenter(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.TopCenter &&
                    (!boolGrid.CenterLeft || boolGrid.BottomRight) &&
                    (!boolGrid.CenterRight || boolGrid.BottomLeft);
        }

        public static Grid3x3<bool?> StraightTopCenterGrid()
        {
            return new Grid3x3<bool?>()
            {
                TopCenter = true,

                CenterLeft = false,
                BottomRight = true,

                CenterRight = false,
                BottomLeft = true,
            };
        }

        public static bool StraightCenterLeft(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterLeft &&
                    (!boolGrid.TopCenter || boolGrid.BottomRight) &&
                    (!boolGrid.BottomCenter || boolGrid.TopRight);
        }

        public static Grid3x3<bool?> StraightCenterLeftGrid()
        {
            return new Grid3x3<bool?>()
            {
                CenterLeft = true,

                TopCenter = false,
                BottomRight = true,

                BottomCenter = false,
                TopRight = true,
            };
        }

        public static bool StraightCenterRight(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterRight &&
                    (!boolGrid.TopCenter || boolGrid.BottomLeft) &&
                    (!boolGrid.BottomCenter || boolGrid.TopLeft);
        }

        public static Grid3x3<bool?> StraightCenterRightGrid()
        {
            return new Grid3x3<bool?>()
            {
                CenterRight = true,

                TopCenter = false,
                BottomLeft = true,

                BottomCenter = false,
                TopLeft = true,
            };
        }

        public static bool StraightBottomCenter(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.BottomCenter &&
                    (!boolGrid.CenterLeft || boolGrid.TopRight) &&
                    (!boolGrid.CenterRight || boolGrid.TopLeft);
        }

        public static Grid3x3<bool?> StraightBottomCenterGrid()
        {
            return new Grid3x3<bool?>()
            {
                BottomCenter = true,

                CenterLeft = false,
                TopRight = true,

                CenterRight = false,
                TopLeft = true,
            };
        }

        public static bool SmallCornerTopLeft(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.TopLeft && 
                    !boolGrid.TopCenter && 
                    !boolGrid.CenterLeft;
        }

        public static Grid3x3<bool?> SmallCornerTopLeftGrid()
        {
            return new Grid3x3<bool?>()
            {
                TopLeft = true,

                TopCenter = false,

                CenterLeft = false,
            };
        }

        public static bool SmallCornerTopRight(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.TopRight && 
                    !boolGrid.TopCenter && 
                    !boolGrid.CenterRight;
        }

        public static Grid3x3<bool?> SmallCornerTopRightGrid()
        {
            return new Grid3x3<bool?>()
            {
                TopRight = true,

                TopCenter = false,

                CenterRight = false,
            };
        }

        public static bool SmallCornerBottomLeft(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.BottomLeft && 
                    !boolGrid.BottomCenter && 
                    !boolGrid.CenterLeft;
        }

        public static Grid3x3<bool?> SmallCornerBottomLeftGrid()
        {
            return new Grid3x3<bool?>()
            {
                BottomLeft = true,

                BottomCenter = false,

                CenterLeft = false,
            };
        }

        public static bool SmallCornerBottomRight(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.BottomRight && 
                    !boolGrid.BottomCenter && 
                    !boolGrid.CenterRight;
        }

        public static Grid3x3<bool?> SmallCornerBottomRightGrid()
        {
            return new Grid3x3<bool?>()
            {
                BottomRight = true,

                BottomCenter = false,

                CenterRight = false,
            };
        }

        public static bool BigCornerTopLeft(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterLeft && 
                    boolGrid.TopCenter && 
                    !boolGrid.BottomRight;
        }

        public static Grid3x3<bool?> BigCornerTopLeftGrid()
        {
            return new Grid3x3<bool?>()
            {
                CenterLeft = true,

                TopCenter = true,

                BottomRight = false,
            };
        }

        public static bool BigCornerTopRight(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterRight && 
                    boolGrid.TopCenter && 
                    !boolGrid.BottomLeft;
        }

        public static Grid3x3<bool?> BigCornerTopRightGrid()
        {
            return new Grid3x3<bool?>()
            {
                CenterRight = true,

                TopCenter = true,

                BottomLeft = false,
            };
        }

        public static bool BigCornerBottomLeft(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterLeft && 
                    boolGrid.BottomCenter && 
                    !boolGrid.TopRight;
        }

        public static Grid3x3<bool?> BigCornerBottomLeftGrid()
        {
            return new Grid3x3<bool?>()
            {
                CenterLeft = true,

                BottomCenter = true,

                TopRight = false,
            };
        }

        public static bool BigCornerBottomRight(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterRight && 
                    boolGrid.BottomCenter && 
                    !boolGrid.TopLeft;
        }

        public static Grid3x3<bool?> BigCornerBottomRightGrid()
        {
            return new Grid3x3<bool?>()
            {
                CenterRight = true,

                BottomCenter = true,

                TopLeft = false,
            };
        }

        
    }

    [Serializable]
    public class AutoTileSpriteAndWalkability
    {
        public Sprite BaseSprite;
        public Sprite OutlineSprite;

        public List<AutoTileWalkability> walkabilityBounds;

        public bool IsWalkable(Vector2 relativePosition)
        {
            return walkabilityBounds.Any(b => b.Contains(relativePosition));
        }
    }

    [Serializable]
    public class AutoTileWalkability : IDraggableRectCompatible
    {
        public Vector2 Size;
        public Vector2 Position;

        public Vector2 BottomLeft => Position;
        public Vector2 BottomRight => new Vector2(Position.x + Size.x, Position.y);
        public Vector2 TopRight => Position + Size;
        public Vector2 TopLeft => new Vector2(Position.x, Position.y + Size.y);

        public float Top => Position.y + Size.y;
        public float Bottom => Position.y;
        public float Left => Position.x;
        public float Right => Position.x + Size.x;

        public Vector2 ConvertToRectSpace(Vector2 position)
        {
            return new Vector2(position.x, 1 - position.y);
        }

        public Vector2 ConvertToGameSpace(Vector2 rectPosition)
        {
            return new Vector2(rectPosition.x, 1 - rectPosition.y);
        }

        public Vector2 GetPosition()
        {
            return Position;
        }

        public Vector2 GetSize()
        {
            return Size;
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public void SetSize(Vector2 size)
        {
            Size = size;
        }

        public bool Contains(Vector2 relativePosition)
        {
            Vector2 convertedToRectSpacePosition = ConvertToRectSpace(relativePosition);

            Rect rect = new Rect() { position = Position, size = Size };

            return rect.Contains(convertedToRectSpacePosition);
        }

        public Vector2 GetClosestPoint(Vector2 relativePosition)
        {
            Vector2 convertedToRectSpacePosition = ConvertToRectSpace(relativePosition);

            Rect rect = new Rect() { position = Position, size = Size };

            Vector2 closestRectSpacePosition = new Vector2(
                Mathf.Clamp(convertedToRectSpacePosition.x, rect.xMin, rect.xMax),
                Mathf.Clamp(convertedToRectSpacePosition.y, rect.yMin, rect.yMax)
            );

            return ConvertToGameSpace(closestRectSpacePosition);
        }
    }
}
