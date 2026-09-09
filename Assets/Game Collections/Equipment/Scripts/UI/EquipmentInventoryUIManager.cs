using System.Collections.Generic;

namespace Equipment.UI
{
    public class EquipmentInventoryUIManager : Singleton<EquipmentInventoryUIManager>
    {
        public List<EquipmentInventoryUI> EquipmentInventories = new();

        public EquipmentInventory GetPriorityInventory(EquipmentInventory exclusion, EquipmentSO equipment)
        {
            EquipmentInventories.RemoveAll(x => x == null);

            EquipmentInventories.Sort();

            foreach (EquipmentInventoryUI inv in EquipmentInventories)
            {
                if (inv != exclusion && inv.gameObject.activeInHierarchy && inv.Inventory.HasFreeSpot(equipment))
                {
                    return inv.Inventory;
                }
            }

            return null;
        }
    }
}
