using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMovement : MonoBehaviour
{
    public BuffableStat Speed { private set; get; }
    public float SpeedValue => Speed.Value;

    [SerializeField]
    protected Transform visualTransform;

    protected float initialXScale;

    private readonly Dictionary<string, float> referenceAndTimeToRemoveBuff = new Dictionary<string, float>();

    protected virtual void Awake()
    {
        initialXScale = visualTransform.localScale.x;
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
    }

    protected virtual void FixedUpdate()
    {
        HandleMovement();
    }

    public abstract void HandleMovement();

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


