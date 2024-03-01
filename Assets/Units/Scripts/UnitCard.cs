using UnityEngine;

namespace Units
{
    public class UnitCard : MonoBehaviour
    {
        public UnitSO unitSO;
        public Rarity rarity;

        public int Level { get; set; }
        public int MaxLevel { get; set; }

        public delegate void OnLevelUp();
        public OnLevelUp OnLevelUpEvent { get; set; }

        public virtual int CostFunction()
        {
            return Level * (int)rarity * 100;
        }

        public virtual bool TryLevelUp(int gold)
        {
            if (Level >= MaxLevel)
                return false;

            if (gold < CostFunction())
                return false;

            Level++;
            OnLevelUpEvent?.Invoke();

            return true;
        }
    }
}

