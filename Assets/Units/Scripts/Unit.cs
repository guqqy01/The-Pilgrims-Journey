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

        public float MovementSpeed => movement.speed;
        public float CurrentHealth => health.currentHealth;
        public float MaxHealth => health.MaxHealth;

        private void Awake()
        {
            movement = GetComponent<Movement>();
            health = GetComponent<Health>();
            shield = GetComponent<Shield>();
        }

        public void Init(UnitSO unitSO, TileStats tileStats, List<Vector2Int> path)
        {
            movement.Init(path, unitSO, tileStats);
            health.Init(unitSO, tileStats);
            shield.Init(unitSO, tileStats);
        }

        public void Damage(int damage)
        {
            int leftDmg = shield.DamageShield(damage);

            if (leftDmg > 0)
                health.ModifyHealth(-leftDmg);
        }
    }

}

