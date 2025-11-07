using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMap
{
    public class MapTile : MonoBehaviour
    {
        public Vector2Int Position { private set; get; }

        public SpriteRenderer highlight;

        public SpriteRenderer highlightLeft;
        public SpriteRenderer highlightUp;
        public SpriteRenderer highlightRight;
        public SpriteRenderer highlightDown;

        public SpriteRenderer highlightLeftUpCorner;
        public SpriteRenderer highlightRightUpCorner;
        public SpriteRenderer highlightLeftDownCorner;
        public SpriteRenderer highlightRightDownCorner;

        public SpriteRenderer graphic;

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

        public void Init(Sprite graphicSprite, Vector2Int position)
        {
            graphic.sprite = graphicSprite;
            Highlighted = false;
            Position = position;
        }

        public void HighlightWithColor(Color color, bool[,] boolArray)
        {
            highlighted = true;

            highlight.gameObject.SetActive(highlighted);
            highlight.color = color.SetAlpha(0.2f);

            highlightLeft.gameObject.SetActive(boolArray[0, 1]);
            highlightLeft.color = color;

            highlightUp.gameObject.SetActive(boolArray[1, 2]);
            highlightUp.color = color;

            highlightRight.gameObject.SetActive(boolArray[2, 1]);
            highlightRight.color = color;

            highlightDown.gameObject.SetActive(boolArray[1, 0]);
            highlightDown.color = color;

            highlightLeftUpCorner.gameObject.SetActive(boolArray[0, 2]);
            highlightLeftUpCorner.color = color;

            highlightRightUpCorner.gameObject.SetActive(boolArray[2, 2]);
            highlightRightUpCorner.color = color;

            highlightLeftDownCorner.gameObject.SetActive(boolArray[0, 0]);
            highlightLeftDownCorner.color = color;

            highlightRightDownCorner.gameObject.SetActive(boolArray[2, 0]);
            highlightRightDownCorner.color = color;
        }
    }
}

