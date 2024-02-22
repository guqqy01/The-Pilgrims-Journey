using UnityEngine;

namespace Units
{
    public interface UnitSpecialAbility
    {
        public Rarity Rarity { get; set; }

        public int Level { get; set; }
        public int MaxLevel { get; set; }

        public delegate void OnLevelUp();
        public OnLevelUp OnLevelUpEvent { get; set; }

        public virtual int CostFunction()
        {
            return Level * (int)Rarity * 100;
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

