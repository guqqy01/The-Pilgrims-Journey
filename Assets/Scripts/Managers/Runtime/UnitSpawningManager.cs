using System;
using System.Collections.Generic;
using GameManagerCalls;
using LocalMap;
using Units;
using UnityEngine;

/// <summary>
/// Spawns allied units on request. It no longer decides WHEN to spawn (SpawnDirector and quests do)
/// and no longer knows about tiles.
/// </summary>
public class UnitSpawningManager : Singleton<UnitSpawningManager>, IOnGameEnd, IOnCompleteLocalMapLevel, IOnClear
{
    public readonly List<Unit> SpawnedUnits = new List<Unit>();

    public delegate void OnUnitSpawned(Unit unit);
    public OnUnitSpawned onUnitSpawned;

    public event Action<Unit> onUnitDied;
    public event Action<Unit> onUnitEscaped;

    /// <param name="powerBuffPercent">Percentage bonus to health and shield (100 = double). 0 for a normal unit.</param>
    /// <param name="scale">Visual scale multiplier, handy for marking a special unit.</param>
    /// <returns>The spawned unit, or null if it couldn't be spawned right now.</returns>
    public Unit SpawnUnit(UnitSO unitSO, float powerBuffPercent = 0f, float scale = 1f)
    {
        if (unitSO == null || unitSO.unitPrefab == null)
            return null;

        if (PathPicker.Instance == null || PathPicker.Instance.PickedDirection == null)
        {
            Debug.LogWarning("UnitSpawningManager: no path picked yet.");
            return null;
        }

        if (LocalMapGenerator.Instance == null || LocalMapGenerator.Instance.Pathway == null)
        {
            Debug.LogWarning("UnitSpawningManager: LocalMapGenerator or pathway missing");
            return null;
        }

        Direction direction = PathPicker.Instance.PickedDirection.Value;
        List<Vector2Int> path = LocalMapGenerator.Instance.Pathway.PathNodes[direction];

        Unit unit = Instantiate(unitSO.unitPrefab, (Vector2)path[0], Quaternion.identity);
        unit.Init(unitSO, path, 0);

        if (powerBuffPercent > 0f)
        {
            // Assumes Multiplier buffs are percentages, as in Monster.Init. Health.Init has already run,
            // and Health.OnMaxHealthChanged heals by the difference when the max goes up.
            Buff buff = new Buff(BuffType.Multiplier, powerBuffPercent, 0f, 0f);
            unit.Stats.Get(StatTypes.Health).AddBuff("Quest", buff);
            unit.Stats.Get(StatTypes.Shield).AddBuff("Quest", new Buff(BuffType.Multiplier, powerBuffPercent, 0f, 0f));
        }

        if (!Mathf.Approximately(scale, 1f))
            unit.transform.localScale *= scale;

        SpawnedUnits.Add(unit);

        unit.Health.onDie += _ => HandleUnitDied(unit);
        unit.Movement.onPathCompleted += () => HandleUnitEscaped(unit);

        onUnitSpawned?.Invoke(unit);
        return unit;
    }

    private void HandleUnitDied(Unit unit)
    {
        SpawnedUnits.Remove(unit);
        onUnitDied?.Invoke(unit);
    }

    // UnitMovement raises onPathCompleted, then calls LocalMapLevelCompletedManager.UnitPassed(),
    // removes the unit from SpawnedUnits and destroys it.
    private void HandleUnitEscaped(Unit unit) => onUnitEscaped?.Invoke(unit);

    private void DestroyUnits()
    {
        foreach (Unit unit in SpawnedUnits.ToArray())
        {
            if (unit != null)
                Destroy(unit.gameObject);
        }

        SpawnedUnits.Clear();
    }

    public void OnGameEnd(string message) => DestroyUnits();
    public void OnCompleteLocalMapLevel() => DestroyUnits();
    public void Clear() => DestroyUnits();
}