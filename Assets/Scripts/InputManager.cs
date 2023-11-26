using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;


    public bool LeftClick => Input.GetMouseButtonDown(0);
    public bool RightClick => Input.GetMouseButtonDown(1);

    private void Awake()
    {
        Instance = this;
    }


}
