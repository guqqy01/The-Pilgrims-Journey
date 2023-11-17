using UnityEngine;

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

