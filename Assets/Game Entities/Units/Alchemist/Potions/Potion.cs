using UnityEngine;

public abstract class Potion : MonoBehaviour
{
    [SerializeField]
    internal float range;

    private Vector2 target;

    private ParticleSystem trailPS;

    private const float heightAbove = 10f;
    private Vector2 arcTarget;

    private const float timeToHit = 1f;

    private float startTime;
    private Vector2 startPos;

    [SerializeField]
    internal LayerMask unitLayerMask;

    private void Awake()
    {
        trailPS = GetComponent<ParticleSystem>();
    }

    public void Init(Vector2 target)
    {
        this.target = target;
        startPos = transform.position;
        startTime = Time.time;
        arcTarget = ((Vector2)transform.position + target) / 2f + new Vector2(0, heightAbove);
    }

    private void Update()
    {
        if (target == null)
            Shatter();

        float t = (Time.time - startTime) / timeToHit;

        transform.position = Vector2.Lerp(Vector2.Lerp(startPos, arcTarget, t), target, t);

        if (t >= 1)
            Shatter();
    }

    public virtual void Shatter()
    {
        Destroy(gameObject);
    }
}