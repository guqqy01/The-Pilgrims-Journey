using System;
using Units;
using UnityEngine;

namespace Monsters
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        public int damage;
        public float speed;

        public bool homing;

        public Transform target;

        public Vector2 direction;

        public LayerMask layerMask;

        public bool oneHit = true;

        private bool hit = false;

        public delegate void OnHit(OnHitInfo hitInfo);
        public OnHit onHit;

        private Vector2 forwardDirection;

        public void Init(int damage, float speed, bool homing, Transform target, Vector2 direction, LayerMask layerMask)
        {
            this.damage = damage;
            this.speed = speed;
            this.homing = homing;
            this.target = target;
            this.direction = direction;
            this.layerMask = layerMask;
        }

        private void Update()
        {
            if (homing && target != null)
            {
                direction = target.position - transform.position;
                
                transform.right = direction;
            }

            transform.position += (Vector3)direction.normalized * speed * Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!hit && 1 << collision.gameObject.layer == layerMask)
            {
                if (collision.TryGetComponent(out Units.Unit unit))
                {
                    hit = true;
                    OnHitInfo info = unit.Damage(damage);

                    onHit?.Invoke(info);

                    Destroy(gameObject);
                }
            }
        }
    }
}


