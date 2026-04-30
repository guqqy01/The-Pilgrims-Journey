using UnityEngine;

public interface IDraggableRectCompatible
{
    public Vector2 GetSize();
    public void SetSize(Vector2 size);

    public Vector2 GetPosition();
    public void SetPosition(Vector2 position);

    public Rect GetRect()
    {
        return new Rect() { position = GetPosition(), size = GetSize() };
    }
}
