using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [SerializeField]
    private InventoryToolbar toolbar;

    private void Update()
    {
        if (InputManager.Instance.OpenCloseInventory)
        {
            OpenCloseInventory();
        }

        if (InputManager.Instance.OpenCloseMap)
        {
            OpenCloseMap();
        }

        if (InputManager.Instance.OpenClosePrizeWindow)
        {
            OpenClosePrizeWindow();
        }

        if (InputManager.Instance.OpenCloseRelicWindow)
        {
            OpenCloseRelicWindow();
        }

        if (InputManager.Instance.OpenCloseSignWindow)
        {
            OpenCloseSignWindow();
        }
    }

    private void OpenCloseInventory()
    {
        
    }

    private void OpenCloseMap()
    {

    }

    private void OpenClosePrizeWindow()
    {

    }

    private void OpenCloseRelicWindow()
    {

    }

    private void OpenCloseSignWindow()
    {

    }
}
