using UnityEngine;
using UI;

public class DamagePopupEffectReciever : MonoBehaviour
{
    private Health health;

    private Shield shield;

    private void Awake()
    {
        if (TryGetComponent(out health))
        {
            health.onHealthChange += HitHealth;
        }

        if (TryGetComponent(out shield))
        {
            shield.onShieldChange += HitShield;
        }
    }

    private void OnDestroy()
    {
        if (shield != null)
        {
            shield.onShieldChange -= HitShield;
        }
        if (health != null)
        {
            health.onHealthChange -= HitHealth;
        }
    }

    public void HitShield(StatChangeInfo shieldChangeInfo)
    {
        if (shieldChangeInfo == null)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        if (shieldChangeInfo.HitInfoType != HitInfoType.Damage || shieldChangeInfo.ChangeAmount <= 0)
            return;

        DamagePopupManager.Instance.SpawnPopup(
            transform.position,
            shieldChangeInfo.ChangeAmount,
            false,
            shieldChangeInfo.HitDirection
        );
    }

    private void HitHealth(StatChangeInfo healthChangeInfo)
    {
        if (healthChangeInfo == null)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        if (healthChangeInfo.HitInfoType != HitInfoType.Damage || healthChangeInfo.ChangeAmount <= 0)
            return;

        DamagePopupManager.Instance.SpawnPopup(
            transform.position,
            healthChangeInfo.ChangeAmount,
            true,
            healthChangeInfo.HitDirection
        );
    }
}
