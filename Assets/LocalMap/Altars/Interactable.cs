using UnityEngine;

public class Interactable : MonoBehaviour
{
    private IInteractable _interactable;

    [SerializeField]
    private KeyCode interactKey = KeyCode.E;

    private void Awake()
    {
        _interactable = GetComponent<IInteractable>();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(interactKey))
            return;

        if (!_interactable.CanInteract())
            return;

        if (_interactable.Interact())
        {
            Debug.LogWarning($"Interaction with {_interactable} unsuccessful");
        }
    }
}

public interface IInteractable
{
    public bool CanInteract();
    public bool Interact();
}