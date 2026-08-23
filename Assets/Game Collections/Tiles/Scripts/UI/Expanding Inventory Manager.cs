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
        if(monsterInventory != null )
            monsterInventory.SetActive(false);
    }

    public void OpenMonsterInventory()
    {
        unitInventory.SetActive(false);
        if (monsterInventory != null)
            monsterInventory.SetActive(true);
    }

    public void CloseInventories()
    {
        unitInventory.SetActive(false);
        if (monsterInventory != null)
            monsterInventory.SetActive(false);
    }
}
