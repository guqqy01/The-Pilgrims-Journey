using GameManagerCalls;
using Heros;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StatContainer : MonoBehaviour
{
    private readonly Dictionary<StatTypes, BuffableStat> stats = new Dictionary<StatTypes, BuffableStat>();

    public event Action OnStatChanged;
    public event Action OnSetup;

    public bool IsSetup { private set; get; } = false;

    private void OnDestroy()
    {
        OnStatChanged = null;
        OnSetup = null;
        foreach (var kvp in stats)
        {
            kvp.Value.onValueChange -= InvokeOnStatChanged;
        }
    }

    private void InvokeOnStatChanged()
    {
        OnStatChanged?.Invoke();
    }

    public BuffableStat Get(StatTypes stat)
    {
        if (stats.TryGetValue(stat, out var buff))
        {
            return buff;
        }
        else
        {
            return null;
        }
    }

    public Buff GetBuff(StatTypes stat)
    {
        return Get(stat);
    }

    public void Setup(BuffableEntitySO buffableEntitySO)
    {
        if (buffableEntitySO == null)
            return;

        foreach (var kvp in stats)
            kvp.Value.onValueChange -= InvokeOnStatChanged;
        stats.Clear();

        foreach (StatTypes stat in Enum.GetValues(typeof(StatTypes)))
        {
            Buff baseBuff = buffableEntitySO.GetBuff(stat);
            if (baseBuff == null) continue;

            var buffable = new BuffableStat(stat.IsIntStat() ? 0 : 0f);
            buffable.AddBuff("EntityBase", baseBuff);
            stats.Add(stat, buffable);
            buffable.onValueChange += InvokeOnStatChanged;
        }
        OnSetup?.Invoke();
        IsSetup = true;
        InvokeOnStatChanged();
    }
}