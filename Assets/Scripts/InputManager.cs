using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    public static InputManager Instance;

    [SerializeField]
    private Camera cam;

    public Vector2 MousePosition => cam.ScreenToWorldPoint(Input.mousePosition);

    public bool LeftClick => Input.GetMouseButtonDown(0);
    public bool RightClick => Input.GetMouseButtonDown(1);


    [SerializeField]
    private KeyCode openCloseInventoryKey = KeyCode.I;
    public bool OpenCloseInventory => Input.GetKeyDown(openCloseInventoryKey);

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (!LeftClick)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Collider2D collider = Physics2D.OverlapPoint(ray.origin);

        if (collider == null)
            return;

        if (collider.TryGetComponent(out InputInteractiveObject interactiveObject))
            interactiveObject.Interact();
    }
}
