using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private InputManager inputManager;

    [SerializeField]
    private GameObject inventoryGO;

    private void Start()
    {
        inputManager = InputManager.Instance;
    }

    private void Update()
    {
        if (inputManager.OpenCloseInventory)
        {
            inventoryGO.SetActive(!inventoryGO.activeInHierarchy);
        }
    }
}
