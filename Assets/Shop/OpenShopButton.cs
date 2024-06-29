using UnityEngine;

public class OpenShopButton : MonoBehaviour
{
    public void OpenShop()
    {
        InventoryToolbar.Instance.OpenByReference("Shop");
    }
}
