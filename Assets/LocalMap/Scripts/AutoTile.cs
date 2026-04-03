using UnityEngine;
using System;
using System.Collections.Generic;

namespace LocalMap
{
    [Serializable]
    public class AutoTile
    {
        public TileType OverTileType;
        public TileType UnderTileType;

        public Sprite OverBase;
        public Sprite UnderBase;

        [Header("Normal")]
        [Header("Straight", order = 1)]
        public Sprite TopCenter;
        public Sprite CenterLeft;
        public Sprite CenterRight;
        public Sprite BottomCenter;

        [Header("Small Corner")]
        public Sprite TopRight;
        public Sprite TopLeft;
        public Sprite BottomLeft;
        public Sprite BottomRight;

        [Header("Big Corner")]
        public Sprite TopAndLeft;
        public Sprite TopAndRight;
        public Sprite BottomAndLeft;
        public Sprite BottomAndRight;

        [Header("Outline")]
        [Header("Straight", order = 1)]
        public Sprite TopCenterOutline;
        public Sprite CenterLeftOutline;
        public Sprite CenterRightOutline;
        public Sprite BottomCenterOutline;

        [Header("Small Corner")]
        public Sprite TopLeftOutline;
        public Sprite TopRightOutline;
        public Sprite BottomLeftOutline;
        public Sprite BottomRightOutline;

        [Header("Big Corner")]
        public Sprite TopAndLeftOutline;
        public Sprite TopAndRightOutline;
        public Sprite BottomAndLeftOutline;
        public Sprite BottomAndRightOutline;

        public List<Sprite> GetNormalSprites(Grid3x3<bool> boolGrid)
        {
            List<Sprite> returnList = new List<Sprite>();

            if (StraightBottomCenter(boolGrid))
            {
                returnList.Add(BottomCenter);
            }

            if (StraightCenterLeft(boolGrid))
            {
                returnList.Add(CenterLeft);
            }

            if (StraightCenterRight(boolGrid))
            {
                returnList.Add(CenterRight);
            }

            if (StraightTopCenter(boolGrid))
            {
                returnList.Add(TopCenter);
            }

            if (SmallCornerBottomLeft(boolGrid))
            {
                returnList.Add(BottomLeft);
            }

            if (SmallCornerBottomRight(boolGrid))
            {
                returnList.Add(BottomRight);
            }

            if (SmallCornerTopLeft(boolGrid))
            {
                returnList.Add(TopLeft);
            }

            if (SmallCornerTopRight(boolGrid))
            {
                returnList.Add(TopRight);
            }

            if (BigCornerBottomLeft(boolGrid))
            {
                returnList.Add(BottomAndLeft);
            }

            if (BigCornerBottomRight(boolGrid))
            {
                returnList.Add(BottomAndRight);
            }

            if (BigCornerTopLeft(boolGrid))
            {
                returnList.Add(TopAndLeft);
            }

            if (BigCornerTopRight(boolGrid))
            {
                returnList.Add(TopAndRight);
            }

            return returnList;
        }

        public List<Sprite> GetOutlineSprites(Grid3x3<bool> boolGrid)
        {
            List<Sprite> returnList = new List<Sprite>();

            if (StraightBottomCenter(boolGrid))
            {
                returnList.Add(BottomCenterOutline);
            }

            if (StraightCenterLeft(boolGrid))
            {
                returnList.Add(CenterLeftOutline);
            }

            if (StraightCenterRight(boolGrid))
            {
                returnList.Add(CenterRightOutline);
            }

            if (StraightTopCenter(boolGrid))
            {
                returnList.Add(TopCenterOutline);
            }

            if (SmallCornerBottomLeft(boolGrid))
            {
                returnList.Add(BottomLeftOutline);
            }

            if (SmallCornerBottomRight(boolGrid))
            {
                returnList.Add(BottomRightOutline);
            }

            if (SmallCornerTopLeft(boolGrid))
            {
                returnList.Add(TopLeftOutline);
            }

            if (SmallCornerTopRight(boolGrid))
            {
                returnList.Add(TopRightOutline);
            }

            if (BigCornerBottomLeft(boolGrid))
            {
                returnList.Add(BottomAndLeftOutline);
            }

            if (BigCornerBottomRight(boolGrid))
            {
                returnList.Add(BottomAndRightOutline);
            }

            if (BigCornerTopLeft(boolGrid))
            {
                returnList.Add(TopAndLeftOutline);
            }

            if (BigCornerTopRight(boolGrid))
            {
                returnList.Add(TopAndRightOutline);
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


}
