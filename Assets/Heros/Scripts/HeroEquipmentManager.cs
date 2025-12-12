using Equipment;
namespace Heros
{
    public class HeroEquipmentManager : Singleton<HeroEquipmentManager>
    {
        private EquipmentSO[] equipment = new EquipmentSO[3];

        public EquipmentSO[] Equipment { get { return equipment; } }



        public Buff GetBuff(HeroStats stat)
        {
            Buff returnBuff = new Buff(stat.IsIntStat());

            foreach(var item in equipment)
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

