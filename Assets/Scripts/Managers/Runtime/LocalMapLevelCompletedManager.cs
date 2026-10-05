using GameManagerCalls;
using GlobalMap;
using UnityEngine;
using Tiles;
using Helper.Wait;
using Monsters;
public class LocalMapLevelCompletedManager : Singleton<LocalMapLevelCompletedManager>, IOnBeforeCreateLocalMap, IOnPlayLocalLevel
{
    private int unitsPassed = 0;

    [SerializeField]
    private AnimationCurve requiredUnits;

    public bool CurrentlyPlayingLocalLevel { private set; get; } = false;

    public int RequiredNumberOfUnitsToPass => Mathf.CeilToInt(requiredUnits.Evaluate(GameManager.Instance.Level));

    private bool tilePlayerFinishedPlaying;


    public delegate void OnUnitPassed(int totalPassed, int requiredToPass);
    public static OnUnitPassed onUnitPassed;

    public delegate void RequiredUnitsPassed();
    public RequiredUnitsPassed onRequiredUnitsPassed;

    private bool hoardStarted;

    private void Start()
    {
        TilePlayerManager.WhenReady(SubscribeToEndOfGameCheck);
    }

    protected override void CleanUp()
    {
        TilePlayerManager.IfReady(UnSubscribeToEndOfGameCheck);
    }

    private void SubscribeToEndOfGameCheck(TilePlayerManager manager)
    {
        manager.noRemainingTilesToPlay += StartEndOfGameCheck;
    }
    private void UnSubscribeToEndOfGameCheck(TilePlayerManager manager)
    {
        manager.noRemainingTilesToPlay -= StartEndOfGameCheck;
    }
    public void OnPlayLocalLevel()
    {
        CurrentlyPlayingLocalLevel = true;
    }

    private void StartEndOfGameCheck()
    {
        tilePlayerFinishedPlaying = true;
    }

    private void Update()
    {
        if (!tilePlayerFinishedPlaying)
            return;

        if (hoardStarted)
            return;

        int UnitsOnMap = UnitSpawningManager.Instance.SpawnedUnits.NotNullCount();
        //Units on map >= Remaining Units Required.
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
        tilePlayerFinishedPlaying = false;
        if (GlobalMapGenerator.Instance.CurrentMapPoint.Type == GlobalMapPointType.Normal)
            RewardManager.Instance.GiveRewards();

        InterfaceCaller.CallInterfaces<IOnCompleteLocalMapLevel>(x => x.OnCompleteLocalMapLevel());

        CurrentlyPlayingLocalLevel = false;
    }

    public void OnBeforeCreateLocalMap()
    {
        unitsPassed = 0;
        tilePlayerFinishedPlaying = false;
        CurrentlyPlayingLocalLevel = false;
        hoardStarted = false;
    }

    
}
