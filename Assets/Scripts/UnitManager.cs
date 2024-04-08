using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class UnitManager : MonoBehaviour
    {
        public static UnitManager Instance { private set; get; }

        [SerializeField]
        private List<UnitSO> unlockableUnits;

        public Dictionary<UnitSO, bool> UnlockedUnits { private set; get; } = new Dictionary<UnitSO, bool>();  

        public void UnlockUnit(UnitSO unit)
        {
            UnlockedUnits[unit] = true;
        }

        private void Awake()
        {
            Instance = this;

            foreach(UnitSO unit in unlockableUnits)
            {
                UnlockedUnits.Add(unit, unit.name == "Peasant");
            }
        }

    }
}
