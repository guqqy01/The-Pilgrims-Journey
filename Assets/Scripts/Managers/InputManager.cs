using GlobalMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GlobalMap.UI;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private GlobalMapUI globalMapUI;
    public GlobalMapUI GlobalMapUI
    {
        get
        {
            if(globalMapUI == null)
                globalMapUI = FindAnyObjectByType<GlobalMapUI>();

            return globalMapUI;
        }
    }

    private Camera cam;
    public Camera Camera
    {
        get
        {
            if(cam == null)
                cam = Camera.main;
            return cam;
        }
    }

    public Vector2 MousePosition => Camera.ScreenToWorldPoint(Input.mousePosition);

    public bool LeftClick => Input.GetMouseButtonDown(0);
    public bool RightClick => Input.GetMouseButtonDown(1);

    public KeyCode OpenCloseGlobalMapUIKeyCode = KeyCode.G;

    [SerializeField]
    private KeyCode openCloseInventoryKey = KeyCode.I;
    public bool OpenCloseInventory => Input.GetKeyDown(openCloseInventoryKey);

    [SerializeField]
    private KeyCode openCloseRelicWindowKey = KeyCode.O;
    public bool OpenCloseRelicWindow => Input.GetKeyDown(openCloseRelicWindowKey);

    [SerializeField]
    private KeyCode openClosePrizeWindowKey = KeyCode.P;
    public bool OpenClosePrizeWindow => Input.GetKeyDown(openClosePrizeWindowKey);

    [SerializeField]
    private KeyCode openCloseMapKey = KeyCode.M;
    public bool OpenCloseMap => Input.GetKeyDown(openCloseMapKey);

    [SerializeField]
    private KeyCode openCloseSignWindowKey = KeyCode.S;
    public bool OpenCloseSignWindow => Input.GetKeyDown(openCloseSignWindowKey);

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    public bool MouseOverUI
    {
        get
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
