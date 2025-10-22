using System;
using UnityEngine;

[Serializable]
public class Buff
{
    [Tooltip("Percentage multiplier. Example: 100 = +100% (doubles the base).")]
    public float Multiplier = 0f;

    [SerializeField] private IntOrFloatObject baseBuff;
    [SerializeField] private IntOrFloatObject additionalBuff;

    public IntOrFloatObject BaseBuff => baseBuff;
    public IntOrFloatObject AdditionalBuff => additionalBuff;

    public bool IsInt
    {
        get
        {
            return baseBuff.IsInt;
        }
    }
    public Buff(bool isInt)
    {
        Multiplier = 0f;

        if (isInt)
        {
            baseBuff = new IntOrFloatObject(0);
            additionalBuff = new IntOrFloatObject(0);
        }
        else
        {
            baseBuff = new IntOrFloatObject(0f);
            additionalBuff = new IntOrFloatObject(0f);
        }
    }

    public Buff(float multiplier, IntOrFloatObject baseBuff, IntOrFloatObject additionalBuff)
    {
        Multiplier = multiplier;
        this.baseBuff = baseBuff;
        this.additionalBuff = additionalBuff;
    }

    /// <summary>
    /// (baseValue + BaseBuff) * (1 + Multiplier/100) + AdditionalBuff
    /// </summary>
    public virtual IntOrFloatObject Value(IntOrFloatObject baseValue)
    {
        float result = ((baseValue + baseBuff) * (1 + Multiplier / 100f) + additionalBuff).FloatValue;

        return baseBuff.IsInt
            ? new IntOrFloatObject(Mathf.RoundToInt(result))
            : new IntOrFloatObject(result);
    }

    public int GetBuffGoldValue(UnitStatsEnum Stat) =>
        BuffValueCalculator.GetValue(this, Stat, out _, out _, out _);

    public static Buff operator +(Buff a, Buff b)
    {
        if (a.IsInt)
        {
            return new Buff(
            a.Multiplier + b.Multiplier,
            Mathf.RoundToInt(a.baseBuff + b.baseBuff),
            Mathf.RoundToInt(a.additionalBuff + b.additionalBuff));
        }
        else
        {
            return new Buff(
            a.Multiplier + b.Multiplier,
            (float)(a.baseBuff + b.baseBuff),
            (float)(a.additionalBuff + b.additionalBuff));
        }
    }

    public static Buff operator *(Buff a, float multiplier)
    {
        if (a.IsInt)
        {
            return new Buff(
                a.Multiplier * multiplier,
                Mathf.RoundToInt(a.baseBuff * multiplier),
                Mathf.RoundToInt(a.additionalBuff * multiplier)
            );
        }
        else
        {
            return new Buff(
            a.Multiplier * multiplier,
            a.baseBuff * multiplier,
            a.additionalBuff * multiplier);
        }
    }

    public override string ToString() =>
        $"Base={BaseBuff}, Add={AdditionalBuff}, Mult={Multiplier}%";
}