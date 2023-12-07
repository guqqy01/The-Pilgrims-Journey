using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;


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


}
