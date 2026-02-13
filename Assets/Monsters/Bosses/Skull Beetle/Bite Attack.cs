using UnityEngine;

public class BiteAttack : Projectile
{
    private static readonly int ImapctHash = Animator.StringToHash("Impact");
    [SerializeField]
    private Animator animator;

    private bool attackActive;
    private bool impactTriggered = false;
    protected override bool AttackActive(Collider2D collision)
    {
        if (!impactTriggered)
        {
            animator.SetTrigger(ImapctHash);
            impactTriggered = true;
        }

        return attackActive;
    }

    public void SetAttack(bool value)
    {
        attackActive = value;
    }

    public void SetAttackToActive()
    {
        attackActive = true;
    }

    public void SetAttackToInActive()
    {
        attackActive = false;
    }

    public void DestroyAttack()
    {
        if(gameObject != null)
            Destroy(gameObject);
    }
}
