

namespace Equipment
{
    public interface EquipmentSpecial
    {
        public void Init(params object[] initParameters);

        public void Update();

        public int GoldValue();
    }
}
