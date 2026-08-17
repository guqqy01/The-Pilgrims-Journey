using System;
using UnityEngine;

public class StatBuffsManager : MonoBehaviour
{
    [SerializeField]
    private StatContainer stats;

    public event Action<StatTypes> OnStatsChanged;
    public event Action OnBinded;

    protected virtual void OnDestroy()
    {
        OnStatsChanged = null;
        OnBinded = null;
    }

    public void Bind(StatContainer stats)
    {
        this.stats = stats;

        OnBinded?.Invoke();
    }

    public void AddBuff(string reference, StatTypes statType, Buff buff)
    {
        if (stats == null)
        {
            Debug.LogWarning($"Stat container not bound yet. Reference: {reference}, Stat Type: {statType}, Buff {buff}");
            return;
        }

        var buffableStat = stats.Get(statType);
        if (buffableStat == null)
        {
            Debug.LogWarning($"HeroStatBuffsManager: attempted to add buff for missing stat {statType}");
            return;
        }

        buffableStat.AddBuff(reference, buff);
        OnStatsChanged?.Invoke(statType);
    }

    public void RemoveBuff(string reference, StatTypes statType)
    {
        if (stats == null)
        {
            Debug.LogWarning($"Stat container not bound yet. Reference: {reference}, Stat Type: {statType}");
            return;
        }

        BuffableStat buffableStat = stats.Get(statType);
        if (buffableStat == null)
        {
            Debug.LogWarning($"HeroStatBuffsManager: attempted to remove buff for missing stat {statType}");
            return;
        }

        buffableStat.RemoveBuff(reference);
        OnStatsChanged?.Invoke(statType);
    }
}


