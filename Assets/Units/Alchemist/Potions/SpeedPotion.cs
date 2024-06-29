using UnityEngine;
using Units;

public class SpeedPotion : Potion
{
    [SerializeField]
    private Buff movementBuff;
    [SerializeField]
    private float lastingTime = 3f;
    public override void Shatter()
    {
        Collider2D[] unitColliders = Physics2D.OverlapCircleAll(transform.position, range, unitLayerMask);

        foreach (Collider2D unitCollider in unitColliders)
        {
            if (unitCollider.TryGetComponent(out Movement movement))
            {
                movement.ModifySpeedForTime("AlchemistPotion", movementBuff, lastingTime);
            }
        }

        base.Shatter();
    }
}
