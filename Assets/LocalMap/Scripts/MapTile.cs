using System.Collections;
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

        public void Init(Sprite graphicSprite, Vector2Int position, TileType tileType = TileType.Grass)
        {
            graphic.sprite = graphicSprite;
            Highlighted = false;
            Position = position;

            TileType = tileType;
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


        public void SetupAutoTiles(AutoTile autoBaseTiles, Grid3x3<bool> boolGrid)
        {
            //Straights

            if (boolGrid.TopCenter &&
                (!boolGrid.CenterLeft || boolGrid.BottomRight) &&
                (!boolGrid.CenterRight || boolGrid.BottomLeft))
            {
                SpawnAutoTile(autoBaseTiles.TopCenter, autoBaseTiles.TopCenterOutline);
            }

            if (boolGrid.CenterLeft && 
                (!boolGrid.TopCenter || boolGrid.BottomRight) && 
                (!boolGrid.BottomCenter || boolGrid.TopRight))
            {
                SpawnAutoTile(autoBaseTiles.CenterLeft, autoBaseTiles.CenterLeftOutline);
            }

            if (boolGrid.CenterRight && 
                (!boolGrid.TopCenter || boolGrid.BottomLeft) && 
                (!boolGrid.BottomCenter || boolGrid.TopLeft))
            {
                SpawnAutoTile(autoBaseTiles.CenterRight, autoBaseTiles.CenterRightOutline);
            }

            if (boolGrid.BottomCenter && 
                (!boolGrid.CenterLeft || boolGrid.TopRight) && 
                (!boolGrid.CenterRight || boolGrid.TopLeft))
            {
                SpawnAutoTile(autoBaseTiles.BottomCenter, autoBaseTiles.BottomCenterOutline);
            }

            //Small Corners

            if (boolGrid.TopLeft && (!boolGrid.TopCenter && !boolGrid.CenterLeft))
            {
                SpawnAutoTile(autoBaseTiles.TopLeft, autoBaseTiles.TopLeftOutline);
            }

            if (boolGrid.TopRight && (!boolGrid.TopCenter && !boolGrid.CenterRight))
            {
                SpawnAutoTile(autoBaseTiles.TopRight, autoBaseTiles.TopRightOutline);
            }

            if (boolGrid.BottomLeft && (!boolGrid.BottomCenter && !boolGrid.CenterLeft))
            {
                SpawnAutoTile(autoBaseTiles.BottomLeft, autoBaseTiles.BottomLeftOutline);
            }

            if (boolGrid.BottomRight && (!boolGrid.BottomCenter && !boolGrid.CenterRight))
            {
                SpawnAutoTile(autoBaseTiles.BottomRight, autoBaseTiles.BottomRightOutline);
            }

            //Big Corners

            //Top And Left
            if (boolGrid.CenterLeft && boolGrid.TopCenter && !boolGrid.BottomRight)
            {
                SpawnAutoTile(autoBaseTiles.TopAndLeft, autoBaseTiles.TopAndLeftOutline);
            }

            //Top And Right
            if (boolGrid.CenterRight && boolGrid.TopCenter && !boolGrid.BottomLeft)
            {
                SpawnAutoTile(autoBaseTiles.TopAndRight, autoBaseTiles.TopAndRightOutline);
            }

            //Bottom And Left
            if (boolGrid.CenterLeft && boolGrid.BottomCenter && !boolGrid.TopRight)
            {
                SpawnAutoTile(autoBaseTiles.BottomAndLeft, autoBaseTiles.BottomAndLeftOutline);
            }

            //Bottom And Right
            if (boolGrid.CenterRight && boolGrid.BottomCenter && !boolGrid.TopLeft)
            {
                SpawnAutoTile(autoBaseTiles.BottomAndRight, autoBaseTiles.BottomAndRightOutline);
            }
        }


        public void SpawnAutoTile(Sprite normal, Sprite outline)
        {
            if(normal != null)
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
            }
        }
    }
}
