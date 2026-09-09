using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class Interactable : MonoBehaviour
{
    [SerializeField]
    private InterfaceReference<IInteractable> interactable;
    [SerializeField]
    private Collider2D interactCollider2D;

    [SerializeField]
    private LayerMask heroMask;

    private bool _inRange;

    [SerializeField]
    private InputActionReference interactAction;
    [SerializeField]
    private UnityEvent interactEvents;

    private void Awake()
    {
        interactAction.action.performed -= TryInteract;
        interactAction.action.performed += TryInteract;
    }

    private void OnDestroy()
    {
        interactAction.action.performed -= TryInteract;
    }

    private void Start()
    {
        interactCollider2D.isTrigger = true;
    }

    private void TryInteract(InputAction.CallbackContext context)
    {
        if (!_inRange)
            return;

        InteractableObject();

        interactEvents.Invoke();
    }

    private void InteractableObject()
    {
        if (!interactable.Interface.CanInteract())
            return;

        interactable.Interface.Interact();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_inRange)
            return;

        if (!heroMask.ContainsLayer(collision.gameObject.layer))
            return;

        _inRange = true;
        interactable.Interface.RaiseHeroRangeChange(_inRange);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_inRange)
            return;

        if (!heroMask.ContainsLayer(collision.gameObject.layer))
            return;

        _inRange = false;
        interactable.Interface.RaiseHeroRangeChange(_inRange);
    }
}
