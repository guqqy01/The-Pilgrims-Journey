using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class UnitManager : MonoBehaviour
    {
        public static UnitManager Instance { private set; get; }

        [SerializeField]
        private List<UnitAndState> initialUnitStates;

        public SerializableDictionary<UnitSO, LockedLevelState> UnitStates { private set; get; } = new SerializableDictionary<UnitSO, LockedLevelState>();

        public List<UnitSO> unlockedUnits
        {
            get
            {
                List<UnitSO> units = new List<UnitSO>();

                foreach (var state in UnitStates)
                {
                    if((int)state.Value > 0)
                    {
                        units.Add(state.Key);
                    }
                }

                return units;
            }
        }

        public void ChangeUnitState(UnitSO unit, LockedLevelState state)
        {
            UnitStates[unit] = state;
        }

        private void Awake()
        {
            Instance = this;

            UnitStates.Clear();

            foreach (var unit in initialUnitStates)
            {
                if (!UnitStates.ContainsKey(unit.unit))
                {
                    UnitStates.Add(unit.unit, unit.state);
                }
            }
        }
    }

    [Serializable]
    public class UnitAndState
    {
        public UnitSO unit;
        public LockedLevelState state;
    }
}

public enum LockedLevelState
{
    Unknown = -1,
    Locked = 0,
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
}
