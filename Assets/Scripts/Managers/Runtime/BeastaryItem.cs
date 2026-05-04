using GlobalMap;
using System;
using System.Collections.Generic;
using Units;


namespace Monsters
{

    [Serializable]
    public class BeastaryItem
    {
        public MonsterSO monster;

        public LockedLevelState LockedState
        {
            get
            {
                int lockState = -1;

                int[] list = BeastaryManager.Instance.NumberOfEncountedMonstersToUpgradeLockState;

                for (int i = 0; i < list.Length; i++)
                {
                    if (encountered >= list[i])
                    {
                        lockState++;
                    }
                    else break;
                }

                return (LockedLevelState)lockState;
            }
        }

        public int encountered;
        public SerializableDictionary<UnitSO, int> unitsAndDamageDelt = new SerializableDictionary<UnitSO, int>();
        public SerializableDictionary<UnitSO, int> unitsAndNumberKilled = new SerializableDictionary<UnitSO, int>();

        public int DamageDealtTotal
        {
            get
            {
                int num = 0;

                foreach (var item in unitsAndDamageDelt)
                {
                    num += item.Value;
                }

                return num;
            }
        }

        public int NumberKilledTotal
        {
            get
            {
                int num = 0;

                foreach (var item in unitsAndNumberKilled)
                {
                    num += item.Value;
                }

                return num;
            }
        }

        public void UnitKilled(UnitSO unit)
        {
            if (unitsAndNumberKilled.ContainsKey(unit))
            {
                unitsAndNumberKilled[unit]++;
            }
            else
            {
                unitsAndNumberKilled.Add(unit, 1);
            }
        }

        public void Unitdamaged(UnitSO unit, int amountOfDmg)
        {
            if (unitsAndDamageDelt.ContainsKey(unit))
            {
                unitsAndDamageDelt[unit] += amountOfDmg;
            }
            else
            {
                unitsAndDamageDelt.Add(unit, amountOfDmg);
            }
        }

        public BeastaryItem(MonsterSO monster)
        {
            this.monster = monster;
        }
    }
}