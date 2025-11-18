using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseMovement : MonoBehaviour
{
    private float initSpeed = 1f;

    public BuffableStat Speed { private set; get; }
    public float SpeedValue => Speed.Value;

    [SerializeField]
    protected Transform visualTransform;

    protected float initialXScale;

    private readonly Dictionary<string, float> referenceAndTimeToRemoveBuff = new Dictionary<string, float>();

    public Rigidbody2D rb { protected set; get; }

    private bool initialized = false;

    protected virtual void Awake()
    {
        initialXScale = visualTransform.localScale.x;
        rb = GetComponent<Rigidbody2D>();
    }

    public void BaseInit(float movementSpeed)
    {
        Speed = new BuffableStat(movementSpeed);
        initialized = true;
    }

    public void BaseInit(BuffableStat movementStat)
    {
        Speed = movementStat;
        initialized = true;
    }

    protected virtual void Start()
    {
        if (!initialized)
        {
            Speed = new BuffableStat(initSpeed);
        }
    }

    protected virtual void Update()
    {
        HandleAdditionalTimedBuff();
    }

    protected virtual void FixedUpdate()
    {
        HandleFixedUpdateMovement();
    }

    public abstract void HandleFixedUpdateMovement();

    private void HandleAdditionalTimedBuff()
    {
        if (referenceAndTimeToRemoveBuff.Count <= 0)
            return;

        List<string> toRemove = new List<string>();

        foreach (KeyValuePair<string, float> kvp in referenceAndTimeToRemoveBuff)
        {
            if (Time.time >= kvp.Value)
            {
                Speed.RemoveBuff(kvp.Key);

                toRemove.Add(kvp.Key);
            }
        }

        if (toRemove.Count <= 0)
            return;

        for (int i = 0; i < toRemove.Count; i++)
        {
            referenceAndTimeToRemoveBuff.Remove(toRemove[i]);
        }
    }

    public void ModifySpeedForTime(string reference, Buff movementBuff, float time)
    {
        Speed.AddBuff(reference, movementBuff);

        if (referenceAndTimeToRemoveBuff.ContainsKey(reference))
        {
            referenceAndTimeToRemoveBuff[reference] = Time.time + time;
        }
        else
        {
            referenceAndTimeToRemoveBuff.Add(reference, Time.time + time);
        }
    }
}


