using UnityEngine;

public static class SpriteExtensions
{
    public static void DrawSpriteInRect(this Sprite sprite, Rect rect)
    {
        if (sprite == null) return;
        Texture2D texture = sprite.texture;
        Rect spriteRect = sprite.rect;
        Rect uvRect = new Rect(
            spriteRect.x / texture.width,
            spriteRect.y / texture.height,
            spriteRect.width / texture.width,
            spriteRect.height / texture.height
        );
        GUI.DrawTextureWithTexCoords(rect, texture, uvRect);
    }
}
