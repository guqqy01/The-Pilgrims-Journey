using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GameManagerCalls;
using System.Collections;
using LocalMap;

public class AltarManager : Singleton<AltarManager>, IOnAfterCreateLocalMap, IOnPlayLocalLevel, IOnCompleteLocalMapLevel
{
    [SerializeField]
    private WeightedChanceList<AltarSO> altarSOs;
    private List<Altar> _altars = new List<Altar>();
    [SerializeField]
    private AltarRewardPoolSO altarRewardPool;
    public AltarRewardPoolSO AltarRewardPool => altarRewardPool;

    [SerializeField]
    private AnimationCurve spawnTimeCurve;

    [SerializeField]
    private Altar altarPrefab;

    [SerializeField]
    private TileType validAltarPlacements;

    private Coroutine _spawnCoroutine;

    public void AfterCreateLocalMap()
    {
        _altars.Clear();
    }

    public void OnCompleteLocalMapLevel()
    {
        StopSpawningAltars();
    }

    public void OnPlayLocalLevel()
    {
        BeginSpawningAltars();
    }

    private IEnumerator SpawnAltars()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnTimeCurve.Evaluate(Random.value));

            SpawnAltar();
        }
    }

    private void SpawnAltar()
    {
        Altar altar = LocalMapGenerator.Instance.SpawnObject(altarPrefab, "Altar", new Vector2Int(2, 3), validAltarPlacements);

        if(altar == null)
            return;

        altar.Setup(altarSOs.GetRandom(), AltarRewardPool.relicRewards.GetRandom());
        AltarSpawnController altarSpawnController = altar.GetComponent<AltarSpawnController>();
        altarSpawnController.StartSpawnAnimation(_altars.Count);

        _altars.Add(altar);
    }

    private void BeginSpawningAltars()
    {
        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);

        _spawnCoroutine = StartCoroutine(SpawnAltars());
    }

    private void StopSpawningAltars()
    {
        if(_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);
    }
}
