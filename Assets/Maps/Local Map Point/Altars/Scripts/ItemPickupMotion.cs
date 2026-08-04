using UnityEngine;

/// <summary>
/// Moves an item from its spawn point toward a target using a "guided missile" steering
/// behavior: it launches outward (away from the target, with some random spread), then
/// continuously turns toward the target while both its speed and turn rate accelerate.
/// The turn-rate limit is what produces the smooth arcing swoop — no separate phases needed.
///
/// Attach this alongside a SpriteRenderer + TrailRenderer. The TrailRenderer will
/// automatically follow since this script moves transform.position every frame.
/// </summary>
public class ItemPickupMotion : MonoBehaviour
{
    private Transform _target;

    [Header("Launch")]
    public float launchAngleVariance = 25f; // random +/- degrees off the pure "away from target" direction
    public float startSpeed = 3f;

    [Header("Speed")]
    public float maxSpeed = 16f;
    public float acceleration = 18f; // units/sec^2

    [Header("Turning")]
    [Tooltip("Turn rate right at launch (degrees/sec). Keep this low so the outward burst reads clearly.")]
    public float startTurnRate = 60f;
    [Tooltip("Turn rate once fully spun up (degrees/sec). This is what lets it snap on target late in the flight.")]
    public float maxTurnRate = 320f;
    [Tooltip("How fast the turn rate itself ramps up, in degrees/sec^2.")]
    public float turnRateAcceleration = 400f;

    [Header("Arrival")]
    public float catchDistance = 0.15f;

    [Header("Polish")]
    public bool faceDirectionOfTravel = true;
    public float rotationOffsetDegrees = 0f; // adjust if your sprite doesn't point "right" by default

    [Header("Visual Layer")]
    public SpriteRenderer spriteRenderer;
    public TrailRenderer trailRenderer;

    Vector2 velocity;
    float currentSpeed;
    float currentTurnRate;
    bool arrived;

    public void Setup(Transform target, int sortingOrder)
    {
        _target = target;

        spriteRenderer.sortingOrder = sortingOrder + 1;
        trailRenderer.sortingOrder = sortingOrder;

        Vector2 awayDir = _target != null
            ? ((Vector2)transform.position - (Vector2)_target.position).normalized
            : Random.insideUnitCircle.normalized;

        if (awayDir == Vector2.zero) awayDir = Vector2.up; // fallback if spawned exactly on the target

        float randomAngle = Random.Range(-launchAngleVariance, launchAngleVariance);
        Vector2 launchDir = Rotate(awayDir, randomAngle);

        currentSpeed = startSpeed;
        currentTurnRate = startTurnRate;
        velocity = launchDir * currentSpeed;
        arrived = false;
    }

    void Update()
    {
        if (arrived || _target == null) return;

        // Both speed and turn rate ramp continuously — no phase boundary, no pops.
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        currentTurnRate = Mathf.Min(currentTurnRate + turnRateAcceleration * Time.deltaTime, maxTurnRate);

        // Steer current heading toward the target, limited by the (accelerating) turn rate.
        Vector2 toTarget = (Vector2)_target.position - (Vector2)transform.position;
        float desiredAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        float newAngle = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, currentTurnRate * Time.deltaTime);

        Vector2 newDir = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
        velocity = newDir * currentSpeed;
        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (faceDirectionOfTravel)
        {
            transform.rotation = Quaternion.Euler(0, 0, newAngle + rotationOffsetDegrees);
        }

        if (Vector2.Distance(transform.position, _target.position) <= catchDistance)
        {
            arrived = true;
            OnArrived();
        }
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    void OnArrived()
    {
        // Hook your pickup logic here — e.g. fire an event, add to inventory, play a pop VFX, etc.
        Destroy(gameObject);
    }
}