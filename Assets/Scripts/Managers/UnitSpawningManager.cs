using System.Collections.Generic;
using UnityEngine;
using Units;
using Tiles;
using GameManagerCalls;
using LocalMap;
using Helper.Wait;
using System.Linq;

public class UnitSpawningManager : Singleton<UnitSpawningManager>, IOnGameEnd, IOnCompleteLocalMapLevel, IOnAfterCreateLocalMap, IOnClear
{
    public readonly List<Unit> SpawnedUnits = new List<Unit>();

    private readonly Dictionary<Tile, float> TileAndSpawnRate = new Dictionary<Tile, float>();

    private readonly Dictionary<Tile, float> tileAndNextSpawnTime = new Dictionary<Tile, float>();

    private float totalSpawnRate;

    private float nextTotalSpawnTime;

    public delegate void OnUnitSpawned(Unit unit);
    public OnUnitSpawned onUnitSpawned;

    private void OnEnable()
    {
        WaitForObjectHelper.WaitForObjectThenPerformAction(() => TilePlayerManager.Instance, Subscribe);
    }

    private void OnDisable()
    {
        if(TilePlayerManager.Instance != null)
            TilePlayerManager.Instance.onPositionChangeCurrentSpawning -= OnPositionChange;
    }

    private void Subscribe()
    {
        TilePlayerManager.Instance.onPositionChangeCurrentSpawning -= OnPositionChange;
        TilePlayerManager.Instance.onPositionChangeCurrentSpawning += OnPositionChange;
    }

    private void Update()
    {
        SpawnUnits();
    }

    private void SetupNewLevel()
    {
        tileAndNextSpawnTime.Clear();
    }

    private void SpawnUnits()
    {
        if (TilePlayerManager.Instance == null)
        {
            Debug.LogWarning("TilePlayerManager.Instance is null");
            return;
        }

        if (!TilePlayerManager.Instance.IsPlaying) return;

        if (PathPicker.Instance == null || PathPicker.Instance.PickedDirection == null)
        {
            Debug.LogWarning("PathPicker not ready");
            return;
        }

        if (Time.time < nextTotalSpawnTime) return;

        if (totalSpawnRate <= 0f) return;

        Direction pathDirection = (Direction)PathPicker.Instance.PickedDirection;

        foreach (var kvp in tileAndNextSpawnTime.ToList())
        {
            var tile = kvp.Key;
            var nextSpawnTime = kvp.Value;
            if (Time.time < nextSpawnTime) continue;
            SpawnUnit(tile, pathDirection);
            break;
        }
    }

    private void OnUnitDie(Health health)
    {
        SpawnedUnits.Remove(health.GetComponent<Unit>());
    }

    private void SpawnUnit(Tile tile, Direction pathDirection)
    {
        if (tile == null || tile.UnitSO == null || tile.UnitSO.unitPrefab == null) return;

        if (LocalMapGenerator.Instance == null || LocalMapGenerator.Instance.pathway == null)
        {
            Debug.LogWarning("LocalMapGenerator or pathway missing");
            return;
        }

        // instantiate
        Unit unit = Instantiate(tile.UnitSO.unitPrefab, (Vector2)LocalMapGenerator.Instance.pathway.PathNodes[pathDirection][0], Quaternion.identity);
        unit.Init(tile, LocalMapGenerator.Instance.pathway.PathNodes[pathDirection], 0);
        SpawnedUnits.Add(unit);

        // subscribe safely (store local tile reference or store onto unit)
        unit.Health.onDie += tile.OnUnitDie;
        unit.Health.onDie += OnUnitDie;
        unit.Movement.onPathCompleted += tile.OnUnitEscape;
        tile.OnUnitSpawn();

        // compute spawn increment using local snapshot of rate
        if (!TileAndSpawnRate.TryGetValue(tile, out float spawnRate) || spawnRate <= 0f)
        {
            // if rate missing, remove and exit
            tileAndNextSpawnTime.Remove(tile);
            TileAndSpawnRate.Remove(tile);
            SetupTotalSpawnRate();
            return;
        }

        tileAndNextSpawnTime[tile] = tileAndNextSpawnTime[tile] + 1f / spawnRate;

        // guard against zero
        if (totalSpawnRate > 0f)
            nextTotalSpawnTime = Time.time + 1f / totalSpawnRate;

        onUnitSpawned?.Invoke(unit);

        if (tile.currentAmountOfUnitsInTile <= 0)
        {
            TileAndSpawnRate.Remove(tile);
            tileAndNextSpawnTime.Remove(tile);
            SetupTotalSpawnRate();
        }
    }

    private void OnPositionChange(Dictionary<Tile, int> spawningTilesAndSize)
    {
        nextTotalSpawnTime = 0;

        Dictionary<Tile, float> tileAndNextSpawnTimeHolder = new Dictionary<Tile, float>();

        foreach (var item in tileAndNextSpawnTime)
        {
            if (spawningTilesAndSize.ContainsKey(item.Key))
            {
                tileAndNextSpawnTimeHolder.Add(item.Key, item.Value);
            }
        }

        tileAndNextSpawnTime.Clear();
        TileAndSpawnRate.Clear();

        foreach (var tileAndSizeKVP in spawningTilesAndSize)
        {
            Tile tile = tileAndSizeKVP.Key;
            if (tileAndNextSpawnTimeHolder.ContainsKey(tile))
            {
                tileAndNextSpawnTime.Add(tile, tileAndNextSpawnTimeHolder[tile]);
            }
            else
            {
                tileAndNextSpawnTime.Add(tile, Time.time);
            }
            
            TileAndSpawnRate.Add(tile, tile.SpawnRateValue * tileAndSizeKVP.Value);
        }

        SetupTotalSpawnRate();
    }

    private void SetupTotalSpawnRate()
    {
        totalSpawnRate = 0f;

        foreach (var tileSpawnRateKVP in TileAndSpawnRate)
        {
            totalSpawnRate += tileSpawnRateKVP.Value;
        }
    }

    private void DestroyUnits()
    {
        foreach (Unit unit in SpawnedUnits)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }

        SpawnedUnits.Clear();
    }

    public void OnGameEnd()
    {
        DestroyUnits();
    }

    public void OnCompleteLocalMapLevel()
    {
        DestroyUnits();
    }

    public void AfterCreateLocalMap()
    {
        SetupNewLevel();
    }

    public void Clear()
    {
        DestroyUnits();
    }
}
