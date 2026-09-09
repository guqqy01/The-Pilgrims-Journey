using System;

public interface IInteractable
{
    event Action<bool> HeroRangeChange;

    public void RaiseHeroRangeChange(bool inRange);
    public bool CanInteract();
    public bool Interact();
}