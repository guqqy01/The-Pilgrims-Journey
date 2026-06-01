using System.Collections.Generic;
using System;
using UnityEngine;

namespace Tiles
{
    [CreateAssetMenu(fileName = "TileGridUISpriteStyle", menuName = "Tiles/TileGridUISpriteStyle", order = 0)]
    public class TileGridUISpriteStyle : ScriptableObject
    {
        [SerializeField] private int spritePixelSize;
        private int previousSpritePixelSize;
        public int SpritePixelSize => spritePixelSize;

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

    [Serializable]
    public class TileGridUISprite
    {
        public Sprite Sprite;
        private Sprite _previous;

        public int priority;

        public void ValidateOrRevert(int spritePixelSize)
        {
            if (Sprite == _previous) return;

            if (ValidateAndClearIfWrongSize(spritePixelSize))
                _previous = Sprite;
            else
                Sprite = _previous;
        }

        // Returns true if the sprite is null or the correct size. Clears and logs an error if not.
        public bool ValidateAndClearIfWrongSize(int spritePixelSize)
        {
            if (Sprite == null) return true;

            if (Sprite.rect.size.x != spritePixelSize || Sprite.rect.size.y != spritePixelSize)
            {
                Debug.LogError($"Sprite {Sprite.name} is {Sprite.rect.size.x}×{Sprite.rect.size.y}px but must be {spritePixelSize}×{spritePixelSize}px.");
                Sprite = null;
                return false;
            }

            return true;
        }

        public bool NotNull { get { return Sprite != null; } }
    }
}
