using LocalMap;
using UnityEngine;

[RequireComponent(typeof(Bounce))]
public class Tumbleweed : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float xForce = 2f;

    bool spawnedLeftOfMap;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        spawnedLeftOfMap = rb.position.x < 0;

        if (spawnedLeftOfMap)
        {
            rb.AddForce(Vector2.right * xForce, ForceMode2D.Impulse);
        }
        else
        {
            rb.AddForce(Vector2.left * xForce, ForceMode2D.Impulse);
        }
    }

    private void Update()
    {
        if (spawnedLeftOfMap)
        {
            if (rb.position.x > LocalMapGenerator.Instance.MapSizeX + 5f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (rb.position.x < -5f)
            {
                Destroy(gameObject);
            }
        }
    }
}
