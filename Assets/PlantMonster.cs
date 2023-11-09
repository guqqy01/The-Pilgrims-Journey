using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


namespace Monsters
{
    public class PlantMonster : MonoBehaviour
    {
        public float range;

        public float attackSpeed;
        private float nextAttackTime;

        public int damage;

        public Projectile projectile;


        public Transform target;

        public LayerMask targetMask;

        public Transform GetClosestTargetInRange()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, targetMask);

            float closestDistance = float.MaxValue;
            Transform closestTarget = null;

            foreach (Collider2D collider in colliders)
            {
                float distance = Vector2.Distance(collider.transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = collider.transform;
                }
            }

            return closestTarget;
        }



        private void Update()
        {
            if (target == null || Vector2.Distance(target.position, transform.position) > range)
                target = GetClosestTargetInRange();

            if (target != null)
            {
                if (Time.time >= nextAttackTime)
                {
                    Projectile project = Instantiate(projectile, transform.position, Quaternion.identity, transform);

                    project.Init(damage, attackSpeed, false, target, target.position - transform.position, targetMask);  

                    nextAttackTime = Time.time + 1f / attackSpeed;
                }
            }
        }
    }

    
}

public class Health : MonoBehaviour
{
    public float maxHealth = 5;
    public float currentHealth = 5;

    public delegate void OnDie();
    public OnDie onDie;

    public delegate void OnHealthChange();
    public OnHealthChange onHealthChange;

    public void ModifyHealth(float amount)
    {
        currentHealth = Mathf.Clamp(amount + currentHealth, 0, maxHealth);

        onHealthChange?.Invoke();

        if (currentHealth == 0 && onDie != null)
            onDie.Invoke();
    }
}

public class Movement : MonoBehaviour
{
    public float speed = 1f;

    public Animator ani;

    public List<Vector2Int> Path { private set; get; }

    private int currentPathIndex = 0;

    public void Init(List<Vector2Int> path)
    {
        Path = path;
    }

    private void Update()
    {
        Vector2 targetPos = Path[currentPathIndex];

        Vector2 direction = targetPos - (Vector2)transform.position;

        transform.position += (Vector3)direction.normalized * speed * Time.deltaTime;
    }
}

public class Projectile : MonoBehaviour
{
    public int damage;
    public float speed;

    public bool homing;

    public Transform target;

    public Vector2 direction;

    public LayerMask layerMask;

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
        }

        transform.position += (Vector3)direction.normalized * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == layerMask)
        {
            Health health = other.GetComponent<Health>();

            health.ModifyHealth(-damage);

            Destroy(gameObject);
        }
    }
}

