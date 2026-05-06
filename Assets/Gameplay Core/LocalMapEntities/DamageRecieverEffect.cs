using UnityEngine;

public abstract class DamageRecieverEffect : MonoBehaviour
{
    public virtual void OnHitHealth(StatChangeInfo statChangeInfo) { }

    public virtual void OnHitShield(StatChangeInfo statChangeInfo) { }
}
