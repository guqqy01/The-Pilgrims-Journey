using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    [RequireComponent(typeof(Movement), typeof(Health), typeof(Shield))]
    public class Unit : MonoBehaviour
    {
        private Movement movement;
        private Health health;
        private Shield shield;

        public float MovementSpeed => movement.Speed;
        public float CurrentHealth => health.currentHealth;
        public float MaxHealth => health.MaxHealth;

        private void Awake()
        {
            movement = GetComponent<Movement>();
            health = GetComponent<Health>();
            shield = GetComponent<Shield>();
        }

        public void Init(UnitSO unitSO, TileStats tileStats, List<EquipmentSO> equipmentSOs, List<Vector2Int> path)
        {
            movement.Init(path, unitSO, tileStats, equipmentSOs);
            health.Init(unitSO, tileStats, equipmentSOs);
            shield.Init(unitSO, tileStats, equipmentSOs);
        }

        public void Damage(int damage)
        {
            int leftDmg = shield.DamageShield(damage);

            if (leftDmg > 0)
                health.ModifyHealth(-leftDmg);
        }
    }

}

