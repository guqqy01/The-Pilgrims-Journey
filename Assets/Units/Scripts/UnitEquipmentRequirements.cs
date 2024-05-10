using System.Collections.Generic;
using System;
using UnityEngine;

namespace Units
{
    [Serializable]
    public struct UnitEquipmentRequirements
    {
        public UnitSO resultedUnit;

        [SerializeField]
        private EquipmentSO equipment;
        public EquipmentSO Equipment => equipment;
    }
}

