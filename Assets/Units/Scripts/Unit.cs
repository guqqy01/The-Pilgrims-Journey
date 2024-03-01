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
        private UnitSpecial special;

        public float MovementSpeed => movement.Speed;
        public float CurrentHealth => health.currentHealth;
        public float MaxHealth => health.MaxHealth;

        private void Awake()
        {
            movement = GetComponent<Movement>();
            health = GetComponent<Health>();
            shield = GetComponent<Shield>();
            special = GetComponent<UnitSpecial>();
        }

        public void Init(UnitCard unitCard, TileStats tileStats, List<Vector2Int> path)
        {
            movement.Init(path, unitCard, tileStats);
            health.Init(unitCard, tileStats);
            shield.Init(unitCard, tileStats);
            special.Init(unitCard.rarity, unitCard.Level);
        }

        public void Damage(int damage)
        {
            int leftDmg = shield.DamageShield(damage);

            if (leftDmg > 0)
                health.ModifyHealth(-leftDmg);
        }
    }

}

