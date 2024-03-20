using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    [CreateAssetMenu()]
    public class UnitSO : ScriptableObject
    {
        [Header("Spawning")]
        public float BaseSpawnRate = 1f;

        [Header("Movement")]
        public float MovementSpeed = 1f;

        [Header("Health")]
        public int baseHealth = 5;

        [Header("Shield")]
        public int Shield = 0;
        /// <summary>
        /// Amount of regeneration over 1 second
        /// </summary>
        public float ShieldRegeneration = 1f;
        /// <summary>
        /// Amount of time since taken damage to start regeneration 
        /// </summary>
        public float ShieldCooldown = 2.25f;

        [Header("Objects")]
        public Unit unitPrefab;

        public Sprite unitSprite;

        public List<UnitLookup> unitLookups;

        public UnitSO GetUnit(List<EquipmentSO> equipmentSOs)
        {
            List<UnitLookup> reversedLookups = unitLookups;
            reversedLookups.Reverse();

            foreach (UnitLookup unitLookup in reversedLookups)
            {
                if (unitLookup.ContainsEquipment(equipmentSOs))
                    return unitLookup.unitSO;
            }

            return this;
        }
    }

    [Serializable]
    public struct UnitLookup
    {
        public UnitSO unitSO;
        public List<EquipmentSO> requiredEquipment;

        public bool ContainsEquipment(List<EquipmentSO> equipmentSOs)
        {
            foreach (EquipmentSO equipment in requiredEquipment)
            {
                if (!equipmentSOs.Contains(equipment))
                    return false;
            }
            return true;
        }
    }
}

