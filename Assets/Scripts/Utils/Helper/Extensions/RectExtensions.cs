using UnityEngine;

public static class RectExtensions
{
    public static bool ContainsWholeRect(this Rect rect, Rect other)
    {
        return rect.max.x >= other.max.x &&
            rect.max.y >= other.max.y &&
            rect.min.x <= other.min.x &&
            rect.min.y <= other.min.y;
    }
}
