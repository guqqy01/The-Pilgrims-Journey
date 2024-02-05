using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement), typeof(Health))]
public class Unit : MonoBehaviour
{
    private UnitSO unitSO;

    private Movement movement;
    private Health health;
    
    public float MovementSpeed => movement.speed;
    public float CurrentHealth => health.currentHealth;
    public float MaxHealth => health.maxHealth;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        health = GetComponent<Health>();
    }

    public void Init(UnitSO unitSO, List<Vector2Int> path)
    {
        this.unitSO = unitSO;
        movement.Init(path, unitSO.MovementSpeed);
        health.Init(unitSO.MaxHealth);
    }
}
