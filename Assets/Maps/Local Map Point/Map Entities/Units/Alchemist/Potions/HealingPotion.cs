using UnityEngine;
using Units;

public class HealingPotion : Potion
{
    [SerializeField]
    private int healAmount = 5; 

    public override void Shatter()
    {
        Collider2D[] unitColliders = Physics2D.OverlapCircleAll(transform.position, range, unitLayerMask);

        foreach (Collider2D unitCollider in unitColliders)
        {
            if(unitCollider.TryGetComponent(out Health health))
            {
                health.ModifyHealth(healAmount);
            }
        }

        base.Shatter();
    }
}
