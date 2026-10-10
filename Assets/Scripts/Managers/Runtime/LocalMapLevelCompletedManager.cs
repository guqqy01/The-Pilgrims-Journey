using GameManagerCalls;
using GlobalMap;
using UnityEngine;
using Helper.Wait;
using Monsters;
using Quests;
using Spawning;

public class LocalMapLevelCompletedManager : Singleton<LocalMapLevelCompletedManager>, IOnBeforeCreateLocalMap, IOnPlayLocalLevel
{
    private int unitsPassed = 0;

    public bool CurrentlyPlayingLocalLevel { private set; get; } = false;

    /// <summary>Now decided by the level plan (SpawnDirector) instead of an AnimationCurve.</summary>
    public int RequiredNumberOfUnitsToPass =>
        SpawnDirector.Instance != null && SpawnDirector.Instance.Plan != null
            ? SpawnDirector.Instance.Plan.RequiredUnits
            : 0;

    private bool spawnPhaseFinished;

    public delegate void OnUnitPassed(int totalPassed, int requiredToPass);
    public static OnUnitPassed onUnitPassed;

    public delegate void RequiredUnitsPassed();
    public RequiredUnitsPassed onRequiredUnitsPassed;

    private bool hoardStarted;

    private void Start()
    {
        SpawnDirector.WhenReady(Subscribe);
    }

    protected override void CleanUp()
    {
        SpawnDirector.IfReady(Unsubscribe);
    }

    private void Subscribe(SpawnDirector director) => director.onSpawnPhaseFinished += OnSpawnPhaseFinished;
    private void Unsubscribe(SpawnDirector director) => director.onSpawnPhaseFinished -= OnSpawnPhaseFinished;

    public void OnPlayLocalLevel()
    {
        CurrentlyPlayingLocalLevel = true;
    }

    private void OnSpawnPhaseFinished()
    {
        spawnPhaseFinished = true;
    }

    private void Update()
    {
        if (!spawnPhaseFinished)
            return;

        if (hoardStarted)
            return;

        int UnitsOnMap = UnitSpawningManager.Instance.SpawnedUnits.NotNullCount();
        // Keep waiting while enough units are still walking to possibly reach the requirement.
        if (UnitsOnMap != 0 && UnitsOnMap >= Mathf.Max(RequiredNumberOfUnitsToPass - unitsPassed, 0))
        {
            return;
        }

        if (unitsPassed < RequiredNumberOfUnitsToPass)
        {
            GameManager.Instance.EndGame("Enough of your pilgrims did not survive the journey");
            return;
        }

        bool? hasBossMonsterBeenKilled = MonsterSpawnManager.Instance.HasBossMonsterBeenKilled;

        if (hasBossMonsterBeenKilled.HasValue && !hasBossMonsterBeenKilled.Value)
            return;

        MonsterSpawnManager.Instance.StartHorde();
        hoardStarted = true;
    }

    public void UnitPassed()
    {
        unitsPassed++;
        onUnitPassed?.Invoke(unitsPassed, RequiredNumberOfUnitsToPass);

        if (unitsPassed >= RequiredNumberOfUnitsToPass)
        {
            onRequiredUnitsPassed?.Invoke();
        }
    }

    public void CompleteLevel()
    {
        spawnPhaseFinished = false;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.FinalizeQuests();
            QuestManager.Instance.GrantRewards();
        }

        if (GlobalMapGenerator.Instance.CurrentMapPoint.Type == GlobalMapPointType.Normal)
            RewardManager.Instance.GiveRewards();

        InterfaceCaller.CallInterfaces<IOnCompleteLocalMapLevel>(x => x.OnCompleteLocalMapLevel());

        CurrentlyPlayingLocalLevel = false;
    }

    public void OnBeforeCreateLocalMap()
    {
        unitsPassed = 0;
        spawnPhaseFinished = false;
        CurrentlyPlayingLocalLevel = false;
        hoardStarted = false;
    }
}