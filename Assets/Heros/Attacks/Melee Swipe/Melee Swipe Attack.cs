using UnityEngine;
using System.Collections.Generic;
using Units;
using Monsters;
public class MeleeSwipeAttack : MonoBehaviour
{
    private Animator ani;

    private Collider2D col2D;

    [SerializeField]
    private LayerMask hitLayerMask;

    private List<GameObject> hitGameObjects;

    private int damage;

    private void Awake()
    {
        ani = GetComponentInChildren<Animator>();
        col2D = GetComponentInChildren<Collider2D>();
    }

    public void Init(int damage)
    {
        this.damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hitLayerMask.ContainsLayer(collision.gameObject.layer))
        {
            if (collision.gameObject.TryGetComponent(out Projectile projectile))
            {
                Destroy(projectile.gameObject);
            }
        }

        if (!hitGameObjects.Contains(collision.gameObject) && hitLayerMask.ContainsLayer(collision.gameObject.layer))
        {
            if(collision.gameObject.TryGetComponent(out Health health))
            {
                health.ModifyHealth(-damage);
            }

            hitGameObjects.Add(collision.gameObject);
        }
    }

    private void DestroyAttack()
    {
        Destroy(gameObject);
    }
}
