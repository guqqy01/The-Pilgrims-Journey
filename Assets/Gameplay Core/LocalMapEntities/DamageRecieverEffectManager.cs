using UnityEngine;

public class DamageRecieverEffectManager : MonoBehaviour
{
    private DamageRecieverEffect[] damageRecieverEffects;

    private Health health;
    private Shield shield;

    private void Awake()
    {
        if (TryGetComponent(out health))
        {
            health.onHealthChange += OnHitHealth;
        }

        if (TryGetComponent(out shield))
        {
            shield.onShieldChange += OnHitShield;
        }

        damageRecieverEffects = GetComponents<DamageRecieverEffect>();
    }

    private void OnDestroy()
    {
        if (health != null)
            health.onHealthChange -= OnHitHealth;

        if (shield != null)
            shield.onShieldChange -= OnHitShield;
    }

    public void OnHitHealth(StatChangeInfo statChangeInfo)
    {
        foreach (var damageRecieverEffects in damageRecieverEffects)
        {
            damageRecieverEffects.OnHitHealth(statChangeInfo);
        }
    }

    public void OnHitShield(StatChangeInfo statChangeInfo)
    {
        foreach (var damageRecieverEffects in damageRecieverEffects)
        {
            damageRecieverEffects.OnHitShield(statChangeInfo);
        }
    }
}