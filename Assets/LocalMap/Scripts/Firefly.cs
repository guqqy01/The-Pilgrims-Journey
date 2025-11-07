using UnityEngine;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Firefly : MonoBehaviour
{
    private Camera cam;

    private Vector2 targetPosition;

    [SerializeField]
    private float rotationSpeed;

    [SerializeField]
    private float rangeToSetup = 1f;

    [SerializeField]
    private MinMaxCurve movementSpeedCurve;
    private float movementSpeed;


    [SerializeField]
    private MinMaxCurve timeToSetupCurve;
    private float timeToSetup;

    [SerializeField]
    private MinMaxCurve outerValue;

    private Rigidbody2D rb;

    private bool leavingMap;

    public delegate void OnDestroy(Firefly fly);
    public OnDestroy onDestroy;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if(Time.time >= timeToSetup)
        {
            Setup();
        }

        while(Vector2.Distance(transform.position, targetPosition) < rangeToSetup)
        {
            if (leavingMap)
            {
                onDestroy?.Invoke(this);
                Destroy(gameObject);
            }

            Setup();
        }

        Vector2 direction = targetPosition - (Vector2)transform.position;
        direction.Normalize();

        float rotationAmount = - Vector3.Cross(direction, transform.right).z;
        rb.angularVelocity = rotationAmount * rotationSpeed;
        transform.position += transform.right * Time.deltaTime * movementSpeed;
    }

    private void Setup()
    {
        targetPosition = cam.ViewportToWorldPoint(new Vector3(outerValue.Evaluate(Random.value), outerValue.Evaluate(Random.value)));
        movementSpeed = movementSpeedCurve.Evaluate(Random.value);
        timeToSetup = Time.time + timeToSetupCurve.Evaluate(Random.value);
    }

    public void LeaveMap()
    {
        leavingMap = true;

        float rotation = Random.Range(0f, 2 * Mathf.PI);
        Vector2 viewportPosition = new Vector2(Mathf.Cos(rotation) + 0.5f, Mathf.Sin(rotation) + 0.5f);

        targetPosition = cam.ViewportToWorldPoint(viewportPosition);
        movementSpeed = movementSpeedCurve.Evaluate(Random.value);
        timeToSetup = float.MaxValue;
    }
}
