using UnityEngine;
using UnityEngine.Events;

public class InputContextHandler : MonoBehaviour, IInputContextListener
{
    [SerializeField]
    private InputContext inputContext;
    private InputContextHandle _inputContextHandle = InputContextHandle.Invalid;

    [SerializeField]
    private bool switchOnEnableDisable = true;

    [SerializeField]
    private bool exclusive;

    [SerializeField]
    [Foldout("Events")]
    private UnityEvent onInputOpenedEvent;
    [SerializeField]
    [Foldout("Events")]
    private UnityEvent onInputClosedEvent;
    [SerializeField]
    [Foldout("Events")]
    private UnityEvent onInputPausedEvent;
    [SerializeField]
    [Foldout("Events")]
    private UnityEvent onInputResumedEvent;
    private void OnEnable()
    {
        if(switchOnEnableDisable)
            InputManager.IfReady(PushInput);
    }

    private void OnDisable()
    {
        if (switchOnEnableDisable)
            InputManager.IfReady(PopInput);
    }

    void IInputContextListener.OnContextPaused()
    {
        onInputPausedEvent?.Invoke();
    }

    void IInputContextListener.OnContextResumed()
    {
        onInputResumedEvent?.Invoke();
    }

    void IInputContextListener.OnContextClosed()
    {
        onInputClosedEvent?.Invoke();
    }

    public void Enable()
    {
        InputManager.IfReady(PushInput);
    }

    public void Disable()
    {
        InputManager.IfReady(PopInput);
    }

    private void PushInput(InputManager inputManager)
    {
        if(_inputContextHandle.IsValid)
            inputManager.PopContext(_inputContextHandle);

        _inputContextHandle = inputManager.PushContext(inputContext, exclusive, this);
        onInputOpenedEvent?.Invoke();
    }

    private void PopInput(InputManager inputManager)
    {
        if (!_inputContextHandle.IsValid)
            inputManager.PopContext(_inputContextHandle);

        _inputContextHandle = InputContextHandle.Invalid;
    }
}