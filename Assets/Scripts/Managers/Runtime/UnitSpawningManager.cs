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
    private readonly Dictionary<UnitTile, float> tileSpawnRates = new Dictionary<UnitTile, float>();
    private readonly Dictionary<UnitTile, float> tileNextSpawnTimes = new Dictionary<UnitTile, float>();

    private float totalSpawnRate;
    private float nextTotalSpawnTime;

    // ---------------------------------------------------------
    //  Unity events
    // ---------------------------------------------------------

    private void OnEnable()
    {
        TilePlayerManager.WhenReady(Subscribe);
    }

    private void OnDisable()
    {
        TilePlayerManager.IfReady(
            manager => manager.onPositionChangeCurrentUnitTiles -= SetupNewSpawnRateValues);
    }

    private void Update()
    {
        HandleSpawnUnits();
    }

    // ---------------------------------------------------------
    //  Event Subscription
    // ---------------------------------------------------------

    private void Subscribe(TilePlayerManager manager)
    {
        manager.onPositionChangeCurrentUnitTiles -= SetupNewSpawnRateValues;
        manager.onPositionChangeCurrentUnitTiles += SetupNewSpawnRateValues;
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
            UnitTile tile = kvp.Key;
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

    private void SpawnUnit(UnitTile tile, Direction pathDirection)
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

        if (tile.CurrentAmountOfUnitsInTile <= 0)
            RemoveTileFromSpawnLists(tile);
    }

    private bool IsTileSpawnValid(UnitTile tile)
    {
        if(tile == null)
            return false;

        if(tile.UnitSO == null)
            return false;

        if (tile.UnitSO.unitPrefab == null)
            return false;

        if (LocalMapGenerator.Instance == null ||LocalMapGenerator.Instance.Pathway == null)
        {
            Debug.LogWarning("LocalMapGenerator or pathway missing");
            return false;
        }

        return true;
    }

    private Unit InstantiateUnit(UnitTile tile, Direction pathDirection)
    {
        var path = LocalMapGenerator.Instance.Pathway.PathNodes[pathDirection];
        Unit unit = Instantiate(tile.UnitSO.unitPrefab, (Vector2)path[0], Quaternion.identity);
        unit.Init(tile, path, 0);
        SpawnedUnits.Add(unit);
        return unit;
    }

    private void RegisterUnitEvents(Unit unit, UnitTile tile)
    {
        unit.Health.onDie += tile.OnUnitDie;
        unit.Health.onDie += OnUnitDie;
        unit.Movement.onPathCompleted += tile.OnUnitEscape;
    }

    private bool TryUpdateNextSpawnTime(UnitTile tile)
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

    private void SetupNewSpawnRateValues(Dictionary<UnitTile, int> spawningTilesAndSize)
    {
        nextTotalSpawnTime = 0;

        // retain old spawn times for still-active tiles
        Dictionary<UnitTile, float> preservedTimes = new Dictionary<UnitTile, float>();

        foreach (var kvp in tileNextSpawnTimes)
        {
            UnitTile tile = kvp.Key;
            if (tile.CurrentAmountOfUnitsInTile > 0 &&
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
            UnitTile tile = kvp.Key;
            if (tile.CurrentAmountOfUnitsInTile <= 0)
                continue;

            // restore or create new spawn time
            tileNextSpawnTimes[tile] = preservedTimes.ContainsKey(tile)
                ? preservedTimes[tile]
                : Time.time;

            tileSpawnRates[tile] = tile.SpawnRate * kvp.Value;
        }

        SetupTotalSpawnRate();
    }

    private void RemoveTileFromSpawnLists(UnitTile tile)
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
            if (kvp.Key.CurrentAmountOfUnitsInTile > 0)
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

    public void OnGameEnd(string message) => DestroyUnits();
    public void OnCompleteLocalMapLevel() => DestroyUnits();
    public void AfterCreateLocalMap() => SetupNewLevel();
    public void Clear() => DestroyUnits();
}
