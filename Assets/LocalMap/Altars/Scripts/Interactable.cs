using Heros;
using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Interactable : MonoBehaviour
{
    private IInteractable _interactable;
    private CircleCollider2D _circleCollider2D;

    [SerializeField]
    private KeyCode interactKey = KeyCode.E;
    [SerializeField]
    private LayerMask heroMask;

    private bool _inRange;

    private void Awake()
    {
        _interactable = GetComponent<IInteractable>();
        if (_interactable == null)
            Debug.LogError($"{name} has an Interactable but no IInteractable component.", this);
        _circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        _circleCollider2D.isTrigger = true;
    }

    private void Update()
    {
        if (!_inRange)
            return;

        if (!Input.GetKeyDown(interactKey))
            return;

        if (!_interactable.CanInteract())
            return;

        if (!_interactable.Interact())
        {
            Debug.LogWarning($"Interaction with {_interactable} unsuccessful");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_inRange)
            return;

        if (!heroMask.ContainsLayer(collision.gameObject.layer))
            return;

        _inRange = true;
        _interactable.RaiseHeroRangeChange(_inRange);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_inRange)
            return;

        if (!heroMask.ContainsLayer(collision.gameObject.layer))
            return;

        _inRange = false;
        _interactable.RaiseHeroRangeChange(_inRange);
    }
}

public interface IInteractable
{
    event Action<bool> HeroRangeChange;

    public void RaiseHeroRangeChange(bool inRange);
    public bool CanInteract();
    public bool Interact();
}