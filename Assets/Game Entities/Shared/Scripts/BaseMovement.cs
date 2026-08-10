using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseMovement : MonoBehaviour
{
    public BuffableStat Speed { private set; get; }
    public float SpeedValue => Speed.Value;

    [SerializeField]
    protected Transform visualTransform;

    private Vector2 previousPosition;

    public Direction LastDirection { protected set; get; }

    protected float initialXScale;

    private readonly Dictionary<string, float> referenceAndTimeToRemoveBuff = new Dictionary<string, float>();

    public Rigidbody2D rb { protected set; get; }

    public bool Moving { set; get; } = true;

    public bool AtTargetPosition
    {
        get
        {
            return GetIfAtTargetPosition();
        }
    }

    protected virtual void Awake()
    {
        if(visualTransform != null)
            initialXScale = visualTransform.localScale.x;
        rb = GetComponent<Rigidbody2D>();
    }

    public void BaseInit(float movementSpeed)
    {
        Speed = new BuffableStat(movementSpeed);
    }

    public void BaseInit(BuffableStat movementStat)
    {
        Speed = movementStat;
    }

    protected virtual void Update()
    {
        HandleAdditionalTimedBuff();
        CalculateLastDirection();
    }

    protected virtual void CalculateLastDirection()
    {
        if (previousPosition == Vector2.zero)
        {
            previousPosition = transform.position;
            return;
        }

        Vector2 currentPosition = previousPosition;

        Vector2 movementVector = currentPosition - previousPosition;

        if (movementVector == Vector2.zero)
            return;

        if(Mathf.Abs(movementVector.x) > Mathf.Abs(movementVector.y))
        {
            LastDirection = 
                movementVector.x > 0 ? 
                Direction.East : 
                Direction.West;
        }
        else
        {
            LastDirection = 
                movementVector.y > 0 ? 
                Direction.North : 
                Direction.South;
        }

        previousPosition = transform.position;

    }

    protected virtual void FixedUpdate()
    {
        if (!Moving)
            return;

        HandleFixedUpdateMovement();
    }

    /// <summary>
    /// Implement this method to handle movement logic in FixedUpdate.
    /// </summary>
    /// <returns>The Movement Vector</returns>
    public abstract void HandleFixedUpdateMovement();

    protected abstract bool GetIfAtTargetPosition();

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


