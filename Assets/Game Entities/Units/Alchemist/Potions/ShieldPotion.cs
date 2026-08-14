using UnityEngine;
using Units;

public class ShieldPotion : Potion
{
    [SerializeField]
    private Buff shieldBuff = new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, true);
    [SerializeField]
    private float lastingTime = 4f;
    public override void Shatter()
    {
        Collider2D[] unitColliders = Physics2D.OverlapCircleAll(transform.position, range, unitLayerMask);

        foreach (Collider2D unitCollider in unitColliders)
        {
            if (unitCollider.TryGetComponent(out Shield shield))
            {
                shield.ModifyShieldForTime("AlchemistPotion", shieldBuff, lastingTime);
            }
        }

        base.Shatter();
    }
}
