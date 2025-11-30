using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bounce : MonoBehaviour
{
    private Rigidbody2D rb;

    private const int numberOfBounces = 3;
    private const float yForce = 3f;
    private const float xForce = 2f;
    private const float gravityScale = 3f;
    private const float timeMultiplier = 0.3f;


    private float timeToNextBounce = 0f;

    private int num = 0;

    private static Vector2 angleRange = new Vector2(-45, 45);
    private float angle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    private void Start()
    {
        angle = Random.Range(angleRange.x, angleRange.y);

        float random = Random.Range(1, 3);

        if (random == 2)
            angle += 180;

        DoBounce();
    }

    void Update()
    {
        if (num - 1 >= numberOfBounces)
            return;

        if (Time.time >= timeToNextBounce)
        {
            DoBounce();

            if (num - 1 >= numberOfBounces)
            {
                Destroy(this);
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0;
            }
        }
    }

    private void DoBounce()
    {
        rb.linearVelocity = new Vector2();
        rb.AddForce(new Vector2(xForce * Mathf.Cos(Mathf.Deg2Rad * angle), yForce * (numberOfBounces - (num - 1))), ForceMode2D.Impulse);
        num++;
        timeToNextBounce = Time.time + timeMultiplier * (numberOfBounces - (num - 1));
    }
}
