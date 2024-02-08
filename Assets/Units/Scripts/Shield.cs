using UnityEngine;

public class Shield : MonoBehaviour
{
    private int baseShield;
    private int additionalShield;
    private float shieldMultiplier = 1;

    public int maxShield => Mathf.CeilToInt((baseShield + additionalShield) * shieldMultiplier);
    public int currentShield { private set; get; } = 5;


    private float baseRegeneration;
    private float regenerationMultiplier = 1;

    public float Regeneration => baseRegeneration * regenerationMultiplier;

    private float baseCooldown;
    private float cooldownMultiplier = 1;

    public float Cooldown => baseCooldown * cooldownMultiplier;

    private float timeToNextRegenPoint;


    public delegate void OnDestroy(int leftOverDmg);
    public OnDestroy onDestroy;

    public delegate void OnShieldChange(int currentShield);
    public OnShieldChange onShieldChange;

    public void Init(UnitSO unitSO, TileStats tileStats)
    {
        baseShield = unitSO.Shield;
        additionalShield = tileStats.additionalShield;
        shieldMultiplier = tileStats.shieldMultiplier;

        baseRegeneration = unitSO.ShieldRegeneration;
        regenerationMultiplier = tileStats.shieldRegenerationMultiplier;

        baseCooldown = unitSO.ShieldCooldown;
        cooldownMultiplier = tileStats.shieldCooldownMultiplier;

        currentShield = maxShield;
        onShieldChange?.Invoke(currentShield);
    }


    private void Update()
    {
        if (currentShield == maxShield) return;

        if (Time.time >= timeToNextRegenPoint)
        {
            ModifyShield(1);
            timeToNextRegenPoint += baseRegeneration;
        }
    }

    public void ModifyShield(int amount)
    {
        int leftOverDmg = - (currentShield + amount);

        currentShield = Mathf.Clamp(amount + currentShield, 0, maxShield);

        onShieldChange?.Invoke(currentShield);

        if (amount < 0) timeToNextRegenPoint = Time.time + baseCooldown;

        if (currentShield == 0)
        {
            onDestroy?.Invoke(leftOverDmg);
        }
    }
}
