using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;
using Tiles;
using GameManagerCalls;

public class SpawningManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot, IOnGameEnd, IOnCompleteLocalMapLevel, IOnAfterCreateLocalMap, IOnClear
{
    public static SpawningManager Instance { private set; get; }

    public List<Unit> SpawnedUnits = new List<Unit>();

    Dictionary<Tile, float> TileAndSpawnRate = new Dictionary<Tile, float>();

    Dictionary<Tile, float> tileAndNextSpawnTime = new Dictionary<Tile, float>();

    private float totalSpawnRate;

    private float nextTotalSpawnTime;

    public delegate void OnUnitSpawned(Unit unit);
    public OnUnitSpawned onUnitSpawned;

    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForObjectHelper.WaitForObject(() => TilePlayerManager.Instance, Subscribe));
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
        tileAndNextSpawnTime = new Dictionary<Tile, float>();
    }

    private void SpawnUnits()
    {
        if (TilePlayerManager.Instance == null)
        {
            Debug.LogWarning("TilePlayerManager.Instance is null");
            return;
        }

        if (!TilePlayerManager.Instance.IsPlaying)
            return;

        if (PathPicker.Instance.PickedDirection == null)
        {
            Debug.LogWarning("TileManager is playing but PathPicker.Instance.PickedDirection is null");
            return;
        }

        if (Time.time < nextTotalSpawnTime)
            return;

        Direction pathDirection = (Direction)PathPicker.Instance.PickedDirection;

        foreach (KeyValuePair<Tile, float> kvp in tileAndNextSpawnTime)
        {
            Tile tile = kvp.Key;
            float nextSpawnTime = kvp.Value;

            if (Time.time < nextSpawnTime)
                continue;

            SpawnUnit(tile, pathDirection);

            break;
        }
    }

    private void SpawnUnit(Tile tile, Direction pathDirection)
    {
        Unit unit = Instantiate(tile.UnitSO.unitPrefab, (Vector2)LocalMapGenerator.Instance.pathway.PathNodes[pathDirection][0], Quaternion.identity);
        unit.Init(tile, LocalMapGenerator.Instance.pathway.PathNodes[pathDirection], 0);
        SpawnedUnits.Add(unit);

        unit.Health.onDie += tile.OnUnitDie;
        unit.Movement.onPathCompleted += tile.OnUnitEscape;

        tileAndNextSpawnTime[tile] = tileAndNextSpawnTime[tile] + 1f / TileAndSpawnRate[tile];

        nextTotalSpawnTime = Time.time + 1f / totalSpawnRate;

        onUnitSpawned?.Invoke(unit);
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

        totalSpawnRate = 0f;

        foreach (var tileSpawnRateKVP in TileAndSpawnRate)
        {
            totalSpawnRate += tileSpawnRateKVP.Value;
        }
    }

    private void DestoryUnits()
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
        DestoryUnits();
    }

    public void OnCompleteLocalMapLevel()
    {
        DestoryUnits();
    }

    public void AfterCreateLocalMap()
    {
        SetupNewLevel();
    }

    public void Clear()
    {
        DestoryUnits();
    }
}
