using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class UnitSO : ScriptableObject
{
    [Header("Spawning")]
    public float BaseSpawnRate = 1f;
    [Header("Movement")]
    public float MovementSpeed = 1f;
    public float SlowResist = 0.2f;
    [Header("Health")]
    public int MaxHealth = 5;
    [Header("Shield")]
    public float Shield = 5f;
    public float ShieldRegeneration = 0.5f;
    public float ShieldCooldown = 1.5f;

    public Unit unitPrefab;

    public Sprite unitSprite;
}
