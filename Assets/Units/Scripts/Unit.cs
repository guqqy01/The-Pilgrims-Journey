using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Units
{
    [RequireComponent(typeof(Movement), typeof(Health), typeof(Shield))]
    [RequireComponent(typeof(CoinDrop))]
    public class Unit : MonoBehaviour
    {
        private Movement movement;
        private Health health;
        private Shield shield;
        private CoinDrop coinDrop;

        public float MovementSpeed => movement.Speed;
        public float CurrentHealth => health.currentHealth;
        public float MaxHealth => health.MaxHealth;

        public ParticleSystem healthPotionParticleSystem;

        private void Awake()
        {
            movement = GetComponent<Movement>();
            health = GetComponent<Health>();
            shield = GetComponent<Shield>();
            coinDrop = GetComponent<CoinDrop>();
        }

        public void Init(Tile tile)
        {

        }

        public void Init(UnitSO unitSO, List<Vector2Int> path, int currentPathIndex)
        {
            movement.Init(path, currentPathIndex, unitSO);
            health.Init(unitSO);
            shield.Init(unitSO);
            coinDrop.Init(unitSO);
        }

        public void Init(Tile tile, List<Vector2Int> path, int currentPathIndex)
        {
            movement.Init(path, currentPathIndex, tile);
            health.Init(tile);
            shield.Init(tile);
            coinDrop.Init(tile.Unit);

            List<EquipmentSO> equipmentSOs = tile.Equipment;

            if(equipmentSOs != null)
                foreach (EquipmentSO equipment in equipmentSOs)
                {
                    if (equipment == null)
                        continue;

                    if (equipment.equipmentSpecialClass == "")
                        continue;

                    ((EquipmentSpecial)(MonoBehaviour)gameObject.AddComponent(Type.GetType(equipment.equipmentSpecialClass))).Init(equipment.Parameters);
                }
        }

        public void Damage(int damage)
        {
            int leftDmg = shield.DamageShield(damage);

            if (leftDmg > 0)
                health.ModifyHealth(-leftDmg);
        }
    }

}

