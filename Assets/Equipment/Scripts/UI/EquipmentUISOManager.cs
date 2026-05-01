using System.Collections.Generic;
using System.Linq;
namespace Equipment.UI
{
    public class EquipmentUISOManager : Singleton<EquipmentUISOManager>
    {
        public List<EquipmentUISO> EquipmentUISOs;

        public EquipmentUISO GetEquipmentUISO(EquipmentSO equipmentSO)
        {
            return EquipmentUISOs.Single(x => x.EquipmentSO == equipmentSO);
        }
    }
}