using GameManagerCalls;
using GlobalMap;
using Helper.Wait;
using Monsters;
using Quests;
using Spawning;
using System.Linq;
using UnityEngine;

public class LocalMapLevelCompletedManager : Singleton<LocalMapLevelCompletedManager>, IOnBeforeCreateLocalMap, IOnPlayLocalLevel
{
    private int unitsPassed = 0;

    public bool CurrentlyPlayingLocalLevel { private set; get; } = false;

    public int RequiredNumberOfUnitsToPass =>
        SpawnDirector.Instance != null && SpawnDirector.Instance.Plan != null
            ? SpawnDirector.Instance.Plan.RequiredUnits
            : 0;

    private bool spawnPhaseFinished;
    private bool requiredQuestsCompleted;
    private bool levelFailed;
    private bool hoardStarted;

    public delegate void OnUnitPassed(int totalPassed, int requiredToPass);
    public static OnUnitPassed onUnitPassed;

    public delegate void RequiredUnitsPassed();
    public RequiredUnitsPassed onRequiredUnitsPassed;

    private void Start()
    {
        SpawnDirector.WhenReady(d => d.onSpawnPhaseFinished += OnSpawnPhaseFinished);
        QuestManager.WhenReady(q =>
        {
            q.OnMainQuestFailed += HandleMainQuestFailed;
            q.OnAllMainQuestsCompleted += HandleAllMainQuestsCompleted;
        });
    }

    protected override void CleanUp()
    {
        SpawnDirector.IfReady(d => d.onSpawnPhaseFinished -= OnSpawnPhaseFinished);
        QuestManager.IfReady(q =>
        {
            q.OnMainQuestFailed -= HandleMainQuestFailed;
            q.OnAllMainQuestsCompleted -= HandleAllMainQuestsCompleted;
        });
    }

    public void OnPlayLocalLevel() => CurrentlyPlayingLocalLevel = true;

    private void OnSpawnPhaseFinished() => spawnPhaseFinished = true;

    private void HandleMainQuestFailed(Quest quest)
    {
        if (!CurrentlyPlayingLocalLevel || levelFailed)
            return;

        levelFailed = true;
        GameManager.Instance.EndGame(quest.FailureReason);
    }

    private void HandleAllMainQuestsCompleted() => requiredQuestsCompleted = true;

    private void Update()
    {
        if (levelFailed || hoardStarted)
            return;

        // The horde only starts once the spawn phase is over AND every required quest is done.
        if (!spawnPhaseFinished || !requiredQuestsCompleted)
            return;

        if (UnitSpawningManager.Instance.SpawnedUnits.NotNullCount() != 0)
            return;

        MonsterSpawnManager.Instance.StartHorde();
        hoardStarted = true;
    }

    public void UnitPassed()
    {
        unitsPassed++;
        onUnitPassed?.Invoke(unitsPassed, RequiredNumberOfUnitsToPass);

        if (QuestManager.Instance != null)
            QuestManager.Instance.NotifyUnitPassed(unitsPassed);

        if (unitsPassed >= RequiredNumberOfUnitsToPass)
            onRequiredUnitsPassed?.Invoke();
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

        GameCalls.CompleteLocalMapLevel();

        CurrentlyPlayingLocalLevel = false;
    }

    public void OnBeforeCreateLocalMap()
    {
        unitsPassed = 0;
        spawnPhaseFinished = false;
        requiredQuestsCompleted = false;
        levelFailed = false;
        CurrentlyPlayingLocalLevel = false;
        hoardStarted = false;
    }
}