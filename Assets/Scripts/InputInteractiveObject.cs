using UnityEngine;

public interface InputInteractiveObject
{
    public Collider2D Collider { get; }
    public void Interact();
}
