using System;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.ParticleSystem;
using UnityEditor;

[Serializable]
public class WeightedChanceObject<T>
{
    public T item;

    public MinMaxCurve weightCurve;

    public int weight
    {
        get
        {
            return Mathf.Max(Mathf.RoundToInt(weightCurve.Evaluate(Random.value, Random.value)), 0);
        }
        set
        {
            weightCurve.constant = value;

            if (weightCurve.mode != ParticleSystemCurveMode.Constant)
                weightCurve.mode = ParticleSystemCurveMode.Constant;
        }
    }


    public int Weight(float time) => Mathf.Max(Mathf.RoundToInt(weightCurve.Evaluate(time, Random.value)), 0);
    public int Weight(float time, float lerpFactor) => Mathf.Max(Mathf.RoundToInt(weightCurve.Evaluate(time, lerpFactor)), 0);
}
