using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PointerManager : Singleton<PointerManager>
{
    private Camera cam;
    private Camera Cam
    {
        get
        {
            if(cam == null)
                cam = Camera.main;

            return cam;
        }
    }

    public Vector2 PointerPosition;

    public Vector2 PointerWorldPosition => Cam.ScreenToWorldPoint(PointerPosition);

    protected override void Awake()
    {
        base.Awake();
        PointerPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if(mouse != null)
        {
            PointerPosition = GetPointerPositionMouse();
        }
        else
        {
            PointerPosition = GetPointerPositionPusher();
        }
    }

    private Vector2 GetPointerPositionMouse()
    {
        if(Mouse.current == null)
            return Vector2.zero;

        return Mouse.current.position.ReadValue();
    }

    private Vector2 GetPointerPositionPusher()
    {
        return PointerPosition + InputManager.UI.PointerMovement.ReadValue<Vector2>();
    }

    public bool PointerOverUI =>
        EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
}
