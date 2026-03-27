using UnityEngine;

public interface IEditorMenuToolbarTab
{
    public string Name { get; }

    public float GetHeight(Rect rect);
    public void DrawTab(Rect rect);
}
