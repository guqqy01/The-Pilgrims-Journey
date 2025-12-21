using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuffableStat: IBuffSource
{
    private readonly IntOrFloatObject baseStat;
    public IntOrFloatObject Value { get; private set; }

    private readonly Dictionary<string, IBuffSource> buffs = new();
    public bool IsIntStat => baseStat.IsInt;

    public Buff Buff => ConvertToBuff();

    public delegate void OnValueChange();
    public OnValueChange onValueChange;

    public BuffableStat(int baseStat) : this(new IntOrFloatObject(baseStat)) { }
    public BuffableStat(float baseStat) : this(new IntOrFloatObject(baseStat)) { }

    public BuffableStat(IntOrFloatObject baseStat)
    {
        this.baseStat = baseStat;
        Value = baseStat;
    }

    public void AddBuff(string reference, IBuffSource buffSource)
    {
        if (buffs.TryGetValue(reference, out var old) && old is BuffableStat oldStat)
            oldStat.onValueChange -= RefreshValue;

        buffs[reference] = buffSource;

        if(buffSource is BuffableStat buffableStat)
            buffableStat.onValueChange += RefreshValue;

        RefreshValue();
    }

    public void RemoveBuff(string reference)
    {
        if (!buffs.TryGetValue(reference, out var buffSource))
            return;

        if(buffSource is BuffableStat buffableStat)
            buffableStat.onValueChange -= RefreshValue;

        buffs.Remove(reference);
        RefreshValue();
    }

    private void RefreshValue()
    {
        if (buffs.Count == 0)
        {
            Value = baseStat;
            return;
        }

        // Combine all buffs
        Buff combined = null;
        foreach (var buffSource in buffs.Values)
        {
            Buff buff = buffSource.Buff;
            combined = combined == null ? buff : combined + buff;
        }

        Value = combined.Value(baseStat);

        onValueChange?.Invoke();
    }

    private Buff ConvertToBuff()
    {
        Buff combined = new Buff(0f, baseStat, baseStat.IsInt ? 0 : 0f);

        foreach(var buffSource in buffs.Values)
        {
            Buff buff = buffSource.Buff;
            combined += buff;
        }

        return combined;
    }

    public IntOrFloatObject Evaluate(IntOrFloatObject baseValue)
    {
        return Value;
    }

    public static implicit operator BuffableStat(int value) => new BuffableStat(value);
    public static implicit operator BuffableStat(float value) => new BuffableStat(value);

    public static implicit operator int(BuffableStat obj) => obj.Value.IntValue;
    public static implicit operator float(BuffableStat obj) => obj.Value.FloatValue;


    public static implicit operator Buff(BuffableStat obj) => obj.ConvertToBuff();
}

public interface IBuffSource
{
    Buff Buff { get; }
}