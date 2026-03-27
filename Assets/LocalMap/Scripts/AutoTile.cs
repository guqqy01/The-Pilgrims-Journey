using UnityEngine;
using System;


namespace LocalMap
{
    [Serializable]
    public class AutoTile
    {
        public TileType OverTileType;

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


        public void SpawnAutoTile(Sprite normal, Sprite outline)
        {
            /*if (normal != null)
            {
                SpriteRenderer normalTile = Instantiate(graphic, transform.position, Quaternion.identity, transform);
                normalTile.sprite = normal;
                normalTile.sortingOrder = 1;
            }

            if (outline != null)
            {
                SpriteRenderer outlineTile = Instantiate(graphic, transform.position, Quaternion.identity, transform);
                outlineTile.sprite = outline;
                outlineTile.sortingOrder = 2;
            }*/
        }

        public static bool StraightTopCenter(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.TopCenter &&
                    (!boolGrid.CenterLeft || boolGrid.BottomRight) &&
                    (!boolGrid.CenterRight || boolGrid.BottomLeft);
        }

        public static bool StraightCenterLeft(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterLeft &&
                    (!boolGrid.TopCenter || boolGrid.BottomRight) &&
                    (!boolGrid.BottomCenter || boolGrid.TopRight);
        }

        public static bool StraightCenterRight(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterRight &&
                    (!boolGrid.TopCenter || boolGrid.BottomLeft) &&
                    (!boolGrid.BottomCenter || boolGrid.TopLeft);
        }

        public static bool StraightBottomCenter(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.BottomCenter &&
                    (!boolGrid.CenterLeft || boolGrid.TopRight) &&
                    (!boolGrid.CenterRight || boolGrid.TopLeft);
        }

        public static bool SmallCornerTopLeft(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.TopLeft && 
                    !boolGrid.TopCenter && 
                    !boolGrid.CenterLeft;
        }

        public static bool SmallCornerTopRight(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.TopRight && 
                    !boolGrid.TopCenter && 
                    !boolGrid.CenterRight;
        }

        public static bool SmallCornerBottomLeft(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.BottomLeft && 
                    !boolGrid.BottomCenter && 
                    !boolGrid.CenterLeft;
        }

        public static bool SmallCornerBottomRight(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.BottomRight && 
                    !boolGrid.BottomCenter && 
                    !boolGrid.CenterRight;
        }

        public static bool BigCornerTopLeft(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterLeft && 
                    boolGrid.TopCenter && 
                    !boolGrid.BottomRight;
        }

        public static bool BigCornerTopRight(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterRight && 
                    boolGrid.TopCenter && 
                    !boolGrid.BottomLeft;
        }

        public static bool BigCornerBottomLeft(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterLeft && 
                    boolGrid.BottomCenter && 
                    !boolGrid.TopRight;
        }

        public static bool BigCornerBottomRight(Grid3x3<bool> boolGrid)
        {
            return  boolGrid.CenterRight && 
                    boolGrid.BottomCenter && 
                    !boolGrid.TopLeft;
        }
    }


}
