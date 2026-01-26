using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuffableStat: IBuffSource
{
    private readonly IntOrFloatObject baseStat;
    public IntOrFloatObject Value { get; private set; }

    private readonly Dictionary<string, AddedBuffAndParameters> buffs = new();
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

    public void AddBuff(string reference, IBuffSource buffSource, AddBuffParameters parameters = AddBuffParameters.All)
    {
        if (buffs.TryGetValue(reference, out var old) && old.BuffSource is BuffableStat oldStat)
            oldStat.onValueChange -= RefreshValue;

        buffs[reference] = new AddedBuffAndParameters(buffSource, parameters);

        if(buffSource is BuffableStat buffableStat)
            buffableStat.onValueChange += RefreshValue;

        RefreshValue();
    }

    public void RemoveBuff(string reference)
    {
        if (!buffs.TryGetValue(reference, out var buffSource))
            return;

        if(buffSource.BuffSource is BuffableStat buffableStat)
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
            if(buffSource == null)
                continue;
            Buff buff = buffSource.GetBuff();
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
            Buff buff = buffSource.GetBuff();
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

[Flags]
public enum AddBuffParameters
{
    None = 0,
    Multiplier = 1,
    Base = 2,
    Additional = 4,
    All = Multiplier | Base | Additional
}

public class AddedBuffAndParameters
{
    public IBuffSource BuffSource { get; set; }
    public AddBuffParameters AddBuffParameters { get; set; }

    public Buff GetBuff()
    {
        if (BuffSource == null)
            return null;

        Buff buff = BuffSource.Buff;

        if (AddBuffParameters == AddBuffParameters.All)
            return buff;

        bool isInt = buff.IsInt;

        float multiplier = 0f;
        IntOrFloatObject baseBuff = new IntOrFloatObject(isInt ? 0 : 0f);
        IntOrFloatObject additionalBuff = new IntOrFloatObject(isInt ? 0 : 0f);

        if (AddBuffParameters.HasFlag(AddBuffParameters.Multiplier))
        {
            multiplier = buff.Multiplier;
        }

        if (AddBuffParameters.HasFlag(AddBuffParameters.Base))
        {
            baseBuff = buff.BaseBuff;
        }

        if (AddBuffParameters.HasFlag(AddBuffParameters.Additional))
        {
            additionalBuff = buff.AdditionalBuff;
        }

        return new Buff(multiplier, baseBuff, additionalBuff);
    }

    public AddedBuffAndParameters(IBuffSource buffSource, AddBuffParameters addBuffParameters = AddBuffParameters.All)
    {
        BuffSource = buffSource;
        AddBuffParameters = addBuffParameters;
    }
}

public interface IBuffSource
{
    Buff Buff { get; }
}