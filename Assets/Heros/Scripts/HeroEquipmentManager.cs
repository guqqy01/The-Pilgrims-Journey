using Equipment;
using UnityEngine;
namespace Heros
{
    [RequireComponent(typeof(EquipmentInventory))]
    public class HeroEquipmentManager : Singleton<HeroEquipmentManager>
    {
        private EquipmentInventory inventory;

        private void Awake()
        {
            inventory = GetComponent<EquipmentInventory>();
        }

        public Buff GetBuff(HeroStats stat)
        {
            Buff returnBuff = new Buff(stat.IsIntStat());

            foreach(var item in inventory.NonNullEquipment)
            {
                if(item.TryGetHeroBuff(stat, out Buff outBuff))
                {
                    returnBuff += outBuff;
                }
            }

            return returnBuff;
        }
    }
}

