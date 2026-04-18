using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class Bounce : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private int numberOfBounces = 3;
    [SerializeField] private float yForce = 3f;
    [SerializeField] private float xForce = 2f;
    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float timeMultiplier = 0.3f;


    private float timeToNextBounce = 0f;

    private int currentNumberOfBounces = 0;

    private static Vector2 angleRange = new Vector2(-45, 45);
    private float angle;

    private bool bouncing = true;

    public Action OnSettled;

    public Action OnBounce;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        ResetBounces();
    }

    public void ResetBounces()
    {
        currentNumberOfBounces = 0;
        bouncing = true;
        rb.gravityScale = gravityScale;
        angle = Random.Range(angleRange.x, angleRange.y);

        float random = Random.Range(1, 3);

        if (random == 2)
            angle += 180;
        DoBounce();
    }

    void Update()
    {
        if (!bouncing)
            return;

        if (currentNumberOfBounces - 1 >= numberOfBounces)
        {
            RemoveBounce();
            return;
        }

        if (Time.time >= timeToNextBounce)
        {
            DoBounce();
            OnBounce?.Invoke();

            if (currentNumberOfBounces - 1 >= numberOfBounces)
            {
                RemoveBounce();
            }
        }
    }

    private void RemoveBounce()
    {
        bouncing = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        OnSettled?.Invoke();
        OnSettled = null;
    }

    private void DoBounce()
    {
        rb.linearVelocity = new Vector2();
        rb.AddForce(new Vector2(xForce * Mathf.Cos(Mathf.Deg2Rad * angle), yForce * (numberOfBounces - (currentNumberOfBounces - 1))), ForceMode2D.Impulse);
        currentNumberOfBounces++;
        timeToNextBounce = Time.time + timeMultiplier * (numberOfBounces - (currentNumberOfBounces - 1));
    }
}
