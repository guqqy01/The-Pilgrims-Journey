using System;
using UnityEngine;

[Serializable]
public class Buff
{
    [SerializeField] private UnitStatsEnum stat;
    public UnitStatsEnum Stat => stat;

    [Tooltip("Percentage multiplier. Example: 100 = +100% (doubles the base).")]
    public float Multiplier = 0f;

    [SerializeField] private IntOrFloatObject baseBuff;
    [SerializeField] private IntOrFloatObject additionalBuff;

    public IntOrFloatObject BaseBuff => baseBuff;
    public IntOrFloatObject AdditionalBuff => additionalBuff;

    private static bool IsIntStat(UnitStatsEnum stat) =>
        stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield;

    public Buff(UnitStatsEnum stat)
    {
        this.stat = stat;
        Multiplier = 0f;

        if (IsIntStat(stat))
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

    public Buff(UnitStatsEnum stat, float multiplier, IntOrFloatObject baseBuff, IntOrFloatObject additionalBuff)
    {
        this.stat = stat;
        Multiplier = multiplier;
        this.baseBuff = baseBuff;
        this.additionalBuff = additionalBuff;
    }

    public Buff(UnitStatsEnum stat, Buff other)
    {
        this.stat = stat;
        Multiplier = other.Multiplier;
        baseBuff = other.BaseBuff;
        additionalBuff = other.AdditionalBuff;
    }

    /// <summary>
    /// (baseValue + BaseBuff) * (1 + Multiplier/100) + AdditionalBuff
    /// </summary>
    public virtual IntOrFloatObject Value(IntOrFloatObject baseValue)
    {
        float result = ((baseValue + baseBuff) * (1 + Multiplier / 100f) + additionalBuff).FloatValue;

        return IsIntStat(stat)
            ? new IntOrFloatObject(Mathf.RoundToInt(result))
            : new IntOrFloatObject(result);
    }

    public int GetBuffGoldValue() =>
        BuffValueCalculator.GetValue(this, out _, out _, out _);

    public static Buff operator +(Buff a, Buff b)
    {
        if (a.Stat != b.Stat)
            throw new ArgumentException($"Cannot add buffs of different stats: {a.Stat} vs {b.Stat}");

        return new Buff(
            a.Stat,
            a.Multiplier + b.Multiplier,
            a.BaseBuff + b.BaseBuff,
            a.AdditionalBuff + b.AdditionalBuff
        );
    }

    public override string ToString() =>
        $"{Stat}: Base={BaseBuff}, Add={AdditionalBuff}, Mult={Multiplier}%";
}