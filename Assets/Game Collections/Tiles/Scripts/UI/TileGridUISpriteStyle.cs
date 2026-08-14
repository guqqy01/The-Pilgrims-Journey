using System.Collections.Generic;
using UnityEngine;

namespace Tiles
{
    [CreateAssetMenu(fileName = "TileGridUISpriteStyle", menuName = "Tiles/TileGridUISpriteStyle", order = 0)]
    public class TileGridUISpriteStyle : ScriptableObject
    {
        [SerializeField] private int spritePixelSize;
        private int previousSpritePixelSize;
        public int SpritePixelSize => spritePixelSize;

        [SerializeField] private int spriteCrossOver = 1;
        public int SpriteCrossOver => spriteCrossOver;

        // Note: odd pixel sizes are supported but require special-case handling in StampOuterCorner.
        // Even sizes are recommended.

        public TileGridUISprite BackgroundFillSprite;

        public TileGridUISprite OuterCornerSprites;
        public TileGridUISprite OuterVerticalSprites;
        public TileGridUISprite OuterHorizontalSprites;

        public TileGridUISprite InnerCornerSprites;
        // Stamped between cells along the vertical axis (left/right borders between columns).
        public TileGridUISprite InnerVerticalSeperationSprites;
        // Stamped between cells along the horizontal axis (top/bottom borders between rows).
        public TileGridUISprite InnerHorizontalSeperationSprites;

        public List<TileGridUISprite> AllSprites
        {
            get
            {
                return new List<TileGridUISprite>()
                {
                    BackgroundFillSprite,
                    OuterCornerSprites,
                    OuterVerticalSprites,
                    OuterHorizontalSprites,
                    InnerCornerSprites,
                    InnerVerticalSeperationSprites,
                    InnerHorizontalSeperationSprites
                };
            }
        }

        private void OnValidate()
        {
            if (previousSpritePixelSize != spritePixelSize)
            {
                previousSpritePixelSize = spritePixelSize;

                foreach (var sprite in AllSprites)
                {
                    sprite.ValidateAndClearIfWrongSize(SpritePixelSize);
                }
            }

            foreach (var sprite in AllSprites)
            {
                sprite.ValidateOrRevert(SpritePixelSize);
            }
        }

        public bool Validate()
        {
            if (spritePixelSize <= 0)
            {
                Debug.LogWarning($"{name}: SpritePixelSize must be greater than zero.");
                return false;
            }

            return true;
        }
    }
}
