using GameManagerCalls;
using Heros;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StatContainer : MonoBehaviour
{
    private readonly Dictionary<StatTypes, BuffableStat> stats = new Dictionary<StatTypes, BuffableStat>();

    public event Action OnStatChanged;

    private void OnDestroy()
    {
        OnStatChanged = null;

        foreach(var kvp in stats)
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

    public void Setup(BuffableEntitySO buffableEntitySO)
    {
        if (buffableEntitySO == null)
            return;

        foreach (StatTypes stat in Enum.GetValues(typeof(StatTypes)))
        {
            Buff baseBuff = buffableEntitySO.GetBuff(stat);
            if (baseBuff == null) continue;

            var buffable = new BuffableStat(stat.IsIntStat() ? 0 : 0f);

            buffable.onValueChange += InvokeOnStatChanged;

            buffable.AddBuff("EntityBase", baseBuff);
            stats.Add(stat, buffable);
        }
    }
}