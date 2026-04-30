using UnityEngine;

public struct BoxVector2IntDimensions
{
    public Vector2 position;
    public Vector2 size;

    public BoxVector2IntDimensions(int top, int bottom, int left, int right)
    {
        size = new Vector2(right + 1 - left, top + 1 - bottom);
        position = new Vector2(left + right, top + bottom) / 2f;
    }
}
