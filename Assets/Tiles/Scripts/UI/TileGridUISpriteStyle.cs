using UnityEngine;

namespace Tiles
{
    [CreateAssetMenu(fileName = "TileGridUISpriteStyle", menuName = "Tiles/TileGridUISpriteStyle", order = 0)]
    public class TileGridUISpriteStyle : ScriptableObject
    {
        [SerializeField]
        private int spritePixelSize;
        private int previousSpritePixelSize;
        public int SpritePixelSize => spritePixelSize;

        /// <summary>
        /// Background Sprite: the main fill of the cell, it is stamped on every cell in the grid.
        /// </summary>
        public Sprite BackgroundFillSprite;
        private Sprite previousBackgroundFillSprite;

        /// <summary>
        /// The outer edge of the grid, it is stamped on only the outer edges of the grid.
        /// </summary>
        public Sprite OuterCornerSprites;
        private Sprite previousOuterCornerSprites;

        /// <summary>
        /// The outer edge of the grid, it is stamped on only the outer edges of the grid.
        /// </summary>
        public Sprite OuterVerticalSprites;
        private Sprite previousOuterVerticalSprites;

        /// <summary>
        /// The outer edge of the grid, it is stamped on only the outer edges of the grid.
        /// </summary>
        public Sprite OuterHorizontalSprites;
        private Sprite previousOuterHorizontalSprites;

        /// <summary>
        /// The inner edge of the grid, it is stamped on only the inner edges of the grid, between the cells.
        /// </summary>
        public Sprite InnerCornerSprites;
        private Sprite previousInnerCornerSprites;

        /// <summary>
        /// The inner edges of the grid, it is stamped between the cells, seperating them vertically.
        /// </summary>
        public Sprite InnerVerticalSeperationSprites;
        private Sprite previousVerticalSeperationSprites;

        /// <summary>
        /// The inner edges of the grid, it is stamped between the cells, seperating them horizontally.
        /// </summary>
        public Sprite InnerHorizontalSeperationSprites;
        private Sprite previousHorizontalSeperationSprites;

        private void OnValidate()
        {
            if (previousSpritePixelSize != spritePixelSize)
            {
                previousSpritePixelSize = spritePixelSize;
                MakeSureSpriteHasCorrectSize(ref BackgroundFillSprite);
                MakeSureSpriteHasCorrectSize(ref OuterCornerSprites);
                MakeSureSpriteHasCorrectSize(ref InnerCornerSprites);
                MakeSureSpriteHasCorrectSize(ref OuterVerticalSprites);
                MakeSureSpriteHasCorrectSize(ref OuterHorizontalSprites);
                MakeSureSpriteHasCorrectSize(ref InnerVerticalSeperationSprites);
                MakeSureSpriteHasCorrectSize(ref InnerHorizontalSeperationSprites);
            }

            IfDifferentCheckIfValidIfNotRevert(ref BackgroundFillSprite, ref previousBackgroundFillSprite);
            IfDifferentCheckIfValidIfNotRevert(ref OuterCornerSprites, ref previousOuterCornerSprites);
            IfDifferentCheckIfValidIfNotRevert(ref InnerCornerSprites, ref previousInnerCornerSprites);
            IfDifferentCheckIfValidIfNotRevert(ref OuterVerticalSprites, ref previousOuterVerticalSprites);
            IfDifferentCheckIfValidIfNotRevert(ref OuterHorizontalSprites, ref previousOuterHorizontalSprites);
            IfDifferentCheckIfValidIfNotRevert(ref InnerVerticalSeperationSprites, ref previousVerticalSeperationSprites);
            IfDifferentCheckIfValidIfNotRevert(ref InnerHorizontalSeperationSprites, ref previousHorizontalSeperationSprites);
        }

        private void IfDifferentCheckIfValidIfNotRevert(ref Sprite newSprite, ref Sprite previous)
        {
            if (previous != newSprite && MakeSureSpriteHasCorrectSize(ref newSprite))
            {
                previous = newSprite;
            }
        }

        private bool MakeSureSpriteHasCorrectSize(ref Sprite sprite)
        {
            if (sprite == null)
                return true;

            if (sprite.rect.size.x != spritePixelSize || sprite.rect.size.y != spritePixelSize)
            {
                Debug.LogError($"Sprite {sprite.name} has size {sprite.rect.size.x}×{sprite.rect.size.y} pixels, but must be {spritePixelSize}×{spritePixelSize} pixels.");
                sprite = null;
                return false;
            }

            return true;
        }

        public bool Validate()
        {
            return spritePixelSize > 0
                && BackgroundFillSprite != null
                && OuterCornerSprites != null
                && InnerCornerSprites != null
                && OuterVerticalSprites != null
                && OuterHorizontalSprites != null
                && InnerVerticalSeperationSprites != null
                && InnerHorizontalSeperationSprites != null;
        } 
    }
}

