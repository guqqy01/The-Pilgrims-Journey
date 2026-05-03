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
    [SerializeField] private float yForcePercentageRandomMultiplier = 0.2f;

    private float timeToNextBounce = 0f;

    private int currentNumberOfBounces = 0;

    [SerializeField]
    private Vector2 angleRange = new Vector2(-45, 45);
    private float angle;

    private bool bouncing = true;

    public Action OnSettled;

    public Action OnBounce;

    [Flags]
    private enum LinearVelocityStopOnBounce
    {
        X = 1,
        Y = 2
    }

    [SerializeField]
    private LinearVelocityStopOnBounce onBounceVelocityStop;

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

        if (Random.value > 0.5f)
            angle += 180;

        DoBounce();
    }

    void Update()
    {
        if (!bouncing)
            return;

        if (Time.time >= timeToNextBounce)
        {
            DoBounce();
            OnBounce?.Invoke();

            if (numberOfBounces > 0 && currentNumberOfBounces - 1 >= numberOfBounces)
            {
                RemoveBounce();
            }
        }
    }

    private void RemoveBounce()
    {
        bouncing = false;

        if(onBounceVelocityStop.HasFlag(LinearVelocityStopOnBounce.X))
            rb.linearVelocityX = 0;
        if (onBounceVelocityStop.HasFlag(LinearVelocityStopOnBounce.Y))
            rb.linearVelocityY = 0;
        rb.gravityScale = 0;

        OnSettled?.Invoke();
        OnSettled = null;
    }

    private void DoBounce()
    {
        if (onBounceVelocityStop.HasFlag(LinearVelocityStopOnBounce.X))
            rb.linearVelocityX = 0;
        if (onBounceVelocityStop.HasFlag(LinearVelocityStopOnBounce.Y))
            rb.linearVelocityY = 0;

        float xForceMultiplier = Mathf.Cos(Mathf.Deg2Rad * angle);

        float yForceMultiplier;

        if(numberOfBounces > 0)
        {
            yForceMultiplier = numberOfBounces - currentNumberOfBounces + 1;
        }
        else
        {
            yForceMultiplier = 1f + Random.Range(-yForcePercentageRandomMultiplier, yForcePercentageRandomMultiplier);
        }


        rb.AddForce(new Vector2(xForce * xForceMultiplier, yForce * yForceMultiplier), ForceMode2D.Impulse);
        currentNumberOfBounces++;
        timeToNextBounce = Time.time + timeMultiplier * (numberOfBounces > 0 ? (numberOfBounces - (currentNumberOfBounces - 1)) : 1f);
    }
}
