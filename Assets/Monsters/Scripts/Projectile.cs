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

        [SerializeField]
        private bool pointInTravelDirection;

        public delegate void OnHit(OnHitInfo hitInfo);
        public OnHit onHit;



        public void Init(int damage, float speed, bool homing, Transform target, Vector2 direction, LayerMask layerMask)
        {
            this.damage = damage;
            this.speed = speed;
            this.homing = homing;
            this.target = target;
            this.direction = direction;
            this.layerMask = layerMask;

            if (pointInTravelDirection)
            {
                direction = target.position - transform.position;

                transform.right = direction;
            }
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
                    OnHitInfo info = unit.Damage(damage, (unit.transform.position - transform.position).normalized);

                    onHit?.Invoke(info);

                    Destroy(gameObject);
                }
            }
        }
    }
}


