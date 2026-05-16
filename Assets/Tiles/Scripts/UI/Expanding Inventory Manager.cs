using UnityEngine;

public class ExpandingInventoryManager : MonoBehaviour
{
    [SerializeField]
    private GameObject unitInventory;
    [SerializeField]
    private GameObject monsterInventory;

    public void OpenUnitInventory()
    {
        unitInventory.SetActive(true);
        monsterInventory.SetActive(false);
    }

    public void OpenMonsterInventory()
    {
        unitInventory.SetActive(false);
        monsterInventory.SetActive(true);
    }

    public void CloseInventories()
    {
        unitInventory.SetActive(false);
        monsterInventory.SetActive(false);
    }
}
