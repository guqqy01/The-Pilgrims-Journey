using UnityEngine;
using Units;

public class ShieldPotion : Potion
{
    [SerializeField]
    private int shieldAmount = 5;
    [SerializeField]
    private float lastingTime = 4f;
    public override void Shatter()
    {
        Collider2D[] unitColliders = Physics2D.OverlapCircleAll(transform.position, range, unitLayerMask);

        foreach (Collider2D unitCollider in unitColliders)
        {
            if (unitCollider.TryGetComponent(out Shield shield))
            {
                shield.ModifyShieldForTime("AlchemistPotion", shieldAmount, lastingTime);
            }
        }

        base.Shatter();
    }
}
