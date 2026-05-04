using System.Collections.Generic;
using UnityEngine;
using Units;
using Tiles;
using GameManagerCalls;
using LocalMap;
using Helper.Wait;
using System.Linq;

public class UnitSpawningManager
    : Singleton<UnitSpawningManager>, IOnGameEnd, IOnCompleteLocalMapLevel, IOnAfterCreateLocalMap, IOnClear
{
    // --- Public fields ---
    public readonly List<Unit> SpawnedUnits = new List<Unit>();
    public delegate void OnUnitSpawned(Unit unit);
    public OnUnitSpawned onUnitSpawned;

    // --- Private fields ---
    private readonly Dictionary<TileUI, float> tileSpawnRates = new Dictionary<TileUI, float>();
    private readonly Dictionary<TileUI, float> tileNextSpawnTimes = new Dictionary<TileUI, float>();

    private float totalSpawnRate;
    private float nextTotalSpawnTime;

    // ---------------------------------------------------------
    //  Unity events
    // ---------------------------------------------------------

    private void OnEnable()
    {
        WaitForHelper.WaitForObjectThenPerformAction(() => TilePlayerManager.Instance, Subscribe);
    }

    private void OnDisable()
    {
        if (TilePlayerManager.Instance != null)
            TilePlayerManager.Instance.onPositionChangeCurrentSpawning -= SetupNewSpawnRateValues;
    }

    private void Update()
    {
        HandleSpawnUnits();
    }

    // ---------------------------------------------------------
    //  Event Subscription
    // ---------------------------------------------------------

    private void Subscribe()
    {
        TilePlayerManager.Instance.onPositionChangeCurrentSpawning -= SetupNewSpawnRateValues;
        TilePlayerManager.Instance.onPositionChangeCurrentSpawning += SetupNewSpawnRateValues;
    }

    // ---------------------------------------------------------
    //  Main spawning loop
    // ---------------------------------------------------------

    private void HandleSpawnUnits()
    {
        if (!IsReadyForSpawning())
            return;

        if (Time.time < nextTotalSpawnTime)
            return;

        Direction pathDirection = (Direction)PathPicker.Instance.PickedDirection;

        foreach (var kvp in tileNextSpawnTimes.ToList())
        {
            TileUI tile = kvp.Key;
            float nextSpawn = kvp.Value;

            if (Time.time >= nextSpawn)
            {
                SpawnUnit(tile, pathDirection);   // Spawns *one* unit per frame
                break;
            }
        }
    }

    private bool IsReadyForSpawning()
    {
        if (TilePlayerManager.Instance == null)
        {
            Debug.LogWarning("TilePlayerManager.Instance is null");
            return false;
        }

        if (!TilePlayerManager.Instance.IsPlaying)
            return false;

        if (PathPicker.Instance?.PickedDirection == null)
        {
            Debug.LogWarning("PathPicker not ready");
            return false;
        }

        if (totalSpawnRate <= 0f)
            return false;

        return true;
    }

    // ---------------------------------------------------------
    //  Spawning
    // ---------------------------------------------------------

    private void SpawnUnit(TileUI tile, Direction pathDirection)
    {
        if (!IsTileSpawnValid(tile))
            return;

        Unit unit = InstantiateUnit(tile, pathDirection);
        RegisterUnitEvents(unit, tile);
        
        tile.OnUnitSpawn();

        if (!TryUpdateNextSpawnTime(tile))
            return;

        UpdateGlobalSpawnTime();
        onUnitSpawned?.Invoke(unit);

        if (tile.TileModel.CurrentAmountOfUnitsInTile <= 0)
            RemoveTileFromSpawnLists(tile);
    }

    private bool IsTileSpawnValid(TileUI tile)
    {
        if(tile == null)
            return false;

        if(tile.UnitSO == null)
            return false;

        if (tile.UnitSO.unitPrefab == null)
            return false;

        if (LocalMapGenerator.Instance == null ||LocalMapGenerator.Instance.pathway == null)
        {
            Debug.LogWarning("LocalMapGenerator or pathway missing");
            return false;
        }

        return true;
    }

    private Unit InstantiateUnit(TileUI tile, Direction pathDirection)
    {
        var path = LocalMapGenerator.Instance.pathway.PathNodes[pathDirection];
        Unit unit = Instantiate(tile.UnitSO.unitPrefab, (Vector2)path[0], Quaternion.identity);
        unit.Init(tile, path, 0);
        SpawnedUnits.Add(unit);
        return unit;
    }

    private void RegisterUnitEvents(Unit unit, TileUI tile)
    {
        unit.Health.onDie += tile.OnUnitDie;
        unit.Health.onDie += OnUnitDie;
        unit.Movement.onPathCompleted += tile.OnUnitEscape;
    }

    private bool TryUpdateNextSpawnTime(TileUI tile)
    {
        if (!tileSpawnRates.TryGetValue(tile, out float rate) || rate <= 0f)
        {
            tileNextSpawnTimes.Remove(tile);
            tileSpawnRates.Remove(tile);
            SetupTotalSpawnRate();
            return false;
        }

        tileNextSpawnTimes[tile] += 1f / rate;
        return true;
    }

    private void UpdateGlobalSpawnTime()
    {
        if (totalSpawnRate > 0f)
            nextTotalSpawnTime = Time.time + 1f / totalSpawnRate;
    }

    private void OnUnitDie(Health health)
    {
        Unit unit = health.GetComponent<Unit>();
        if (unit != null)
            SpawnedUnits.Remove(unit);
    }

    // ---------------------------------------------------------
    //  Setup & Updates
    // ---------------------------------------------------------

    private void SetupNewLevel()
    {
        tileNextSpawnTimes.Clear();
    }

    private void SetupNewSpawnRateValues(Dictionary<TileUI, int> spawningTilesAndSize)
    {
        nextTotalSpawnTime = 0;

        // retain old spawn times for still-active tiles
        Dictionary<TileUI, float> preservedTimes = new Dictionary<TileUI, float>();

        foreach (var kvp in tileNextSpawnTimes)
        {
            TileUI tile = kvp.Key;
            if (tile.TileModel.CurrentAmountOfUnitsInTile > 0 &&
                spawningTilesAndSize.ContainsKey(tile))
            {
                preservedTimes[tile] = kvp.Value;
            }
        }

        // reset dictionaries
        tileNextSpawnTimes.Clear();
        tileSpawnRates.Clear();

        foreach (var kvp in spawningTilesAndSize)
        {
            TileUI tile = kvp.Key;
            if (tile.TileModel.CurrentAmountOfUnitsInTile <= 0)
                continue;

            // restore or create new spawn time
            tileNextSpawnTimes[tile] = preservedTimes.ContainsKey(tile)
                ? preservedTimes[tile]
                : Time.time;

            tileSpawnRates[tile] = tile.SpawnRateValue * kvp.Value;
        }

        SetupTotalSpawnRate();
    }

    private void RemoveTileFromSpawnLists(TileUI tile)
    {
        tileSpawnRates.Remove(tile);
        tileNextSpawnTimes.Remove(tile);
        SetupTotalSpawnRate();
    }

    private void SetupTotalSpawnRate()
    {
        totalSpawnRate = 0f;

        foreach (var kvp in tileSpawnRates)
        {
            if (kvp.Key.TileModel.CurrentAmountOfUnitsInTile > 0)
                totalSpawnRate += kvp.Value;
        }
    }

    // ---------------------------------------------------------
    //  Cleanup
    // ---------------------------------------------------------

    private void DestroyUnits()
    {
        foreach (Unit unit in SpawnedUnits)
        {
            if (unit != null)
                Destroy(unit.gameObject);
        }

        SpawnedUnits.Clear();
    }

    public void OnGameEnd() => DestroyUnits();
    public void OnCompleteLocalMapLevel() => DestroyUnits();
    public void AfterCreateLocalMap() => SetupNewLevel();
    public void Clear() => DestroyUnits();
}
