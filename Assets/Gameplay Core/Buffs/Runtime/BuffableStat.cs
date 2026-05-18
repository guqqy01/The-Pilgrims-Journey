using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BuffableStat: IBuffSource, ISerializationCallbackReceiver, IDisposable
{
    // made non-readonly so we can restore value after Unity deserialization
    private IntOrFloatObject baseStat;
    public IntOrFloatObject Value { get; private set; }

    private readonly Dictionary<string, AddedBuffAndParameters> buffs = new();
    public bool IsIntStat => baseStat.IsInt;

    // Guard to avoid recursive re-entry when computing Buff from other BuffableStats
    [NonSerialized]
    private bool _isComputingBuff = false;

    public Buff Buff => ConvertToBuff();

    public delegate void OnValueChange();
    public OnValueChange onValueChange;

    // Serialization backing fields
    [SerializeField]
    private IntOrFloatObject serializedBaseStat;

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

    public void ClearAllBuffs()
    {
        List<string> buffReferences = buffs.Select(x => x.Key).ToList();

        foreach(var buffReference in buffReferences)
        {
            RemoveBuff(buffReference);
        }
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
            if (buff == null)
                continue;
            combined = combined == null ? buff : combined + buff;
        }

        if (combined == null)
        {
            // No valid buffs provided; fallback to baseStat
            Value = baseStat;
        }
        else
        {
            Value = combined.Value(baseStat);
        }

        onValueChange?.Invoke();
    }

    private Buff ConvertToBuff()
    {
        // Prevent recursive re-entry which can happen when BuffableStat.Buff is requested
        // while computing another BuffableStat that depends on this one.
        if (_isComputingBuff)
            return null;

        _isComputingBuff = true;
        try
        {
            // Start from neutral buff
            // Ensure baseStat is initialized
            var bs = baseStat;

            Buff combined = new Buff(0f, bs, bs.IsInt ? new IntOrFloatObject(0) : new IntOrFloatObject(0f));

            foreach(var buffSource in buffs.Values)
            {
                if (buffSource == null)
                    continue;
                Buff buff = buffSource.GetBuff();
                if (buff == null)
                    continue;
                combined += buff;
            }

            return combined;
        }
        finally
        {
            _isComputingBuff = false;
        }
    }

    // Simple serialization: preserve baseStat only. Dynamic buffs are runtime-only and won't be persisted.
    public void OnBeforeSerialize()
    {
        serializedBaseStat = baseStat;
    }

    public void OnAfterDeserialize()
    {
        // Restore baseStat after Unity deserialization
        baseStat = serializedBaseStat;
        Value = baseStat;
    }

    public void Dispose()
    {
        onValueChange = null;
    }

    public static implicit operator BuffableStat(int value) => new BuffableStat(value);
    public static implicit operator BuffableStat(float value) => new BuffableStat(value);

    public static implicit operator int(BuffableStat obj) => obj.Value.IntValue;
    public static implicit operator float(BuffableStat obj) => obj.Value.FloatValue;


    public static implicit operator Buff(BuffableStat obj) => obj.ConvertToBuff();
}
