using UnityEngine;
using UI;
using System.Collections.Generic;

[RequireComponent(typeof(DamageRecieverEffectManager))]
[DisallowMultipleComponent]
public class DamagePopupEffectReciever : DamageRecieverEffect
{
    public override void OnHitShield(StatChangeInfo shieldChangeInfo)
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

    public override void OnHitHealth(StatChangeInfo healthChangeInfo)
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

