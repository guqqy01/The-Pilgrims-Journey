using System;
using UnityEngine;

namespace Tiles
{
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
