using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;

public class ShroomMonster : MonoBehaviour
{
    [SerializeField]
    private float range = 1f;

    [SerializeField]
    private float attackSpeed = 1f;

    private float nextAttackTime;
    [SerializeField]
    private int damage = 1;

    public LayerMask targetMask;

    [SerializeField]
    private Animator ani;

    [SerializeField]
    private ParticleSystem ps;

    private void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, targetMask);

            if (colliders.Length > 0)
            {
                ani.SetTrigger("Attack");

                nextAttackTime = Time.time + 1f / attackSpeed;
            }
        }
    }

    public void Attack()
    {
        ps.Play();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, targetMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (1 << colliders[i].gameObject.layer == targetMask)
            {
                if (colliders[i].TryGetComponent(out Units.Unit unit))
                {
                    unit.Damage(damage);
                }
            }
        }
    }
}
