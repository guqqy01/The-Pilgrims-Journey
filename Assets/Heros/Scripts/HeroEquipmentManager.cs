using Equipment;
using UnityEngine;
using GameManagerCalls;
namespace Heros
{
    [RequireComponent(typeof(EquipmentInventory))]
    public class HeroEquipmentManager : Singleton<HeroEquipmentManager>, IOnAfterCreateLocalMap, IOnCompleteLocalMapLevel, IOnPlayLocalLevel
    {
        private EquipmentInventory inventory;
        public EquipmentInventory Inventory => inventory;
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

        public void AfterCreateLocalMap()
        {
            inventory.Unlock();
        }

        public void OnCompleteLocalMapLevel()
        {
            inventory.Unlock();
        }

        public void OnPlayLocalLevel()
        {
            inventory.Lock();
        }
    }
}

