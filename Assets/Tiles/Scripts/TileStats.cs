using UnityEngine;

public class TileStats : MonoBehaviour
{
    [Header("Spawning")]
    public float spawningMultiplier = 1f;

    [Header("Movement")]
    public float movementMultiplier = 1f;

    [Header("Health")]
    public int additionalHealth = 0;
    public float healthMultiplier = 1f;

    [Header("Shield")]
    public int additionalShield = 0;
    public float shieldMultiplier = 1f;

    public float shieldRegenerationMultiplier = 1f;
    public float shieldCooldownMultiplier = 1f;
}
