using GameManagerCalls;
using GlobalMap;
using Unity.VisualScripting;
using UnityEngine;
using static GameManager;
using System.Collections.Generic;
using System.Linq;
using Tiles;
using Helper.Wait;
using Heros;
using static LocalMapLevelCompletedManager;
using Monsters;
using LocalMap;
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

    private void Start()
    {
        WaitForObjectHelper.WaitForObjectThenPerformAction(() => TilePlayerManager.Instance, SubscribeToEndOfGameCheck, 1f);
    }

    private void SubscribeToEndOfGameCheck()
    {
        TilePlayerManager.Instance.noRemainingTilesToPlay += StartEndOfGameCheck;
    }

    public void OnPlayLocalLevel()
    {
        CurrentlyPlayingLocalLevel = true;
    }

    protected override void CleanUp()
    {
        if (TilePlayerManager.Instance != null)
            TilePlayerManager.Instance.noRemainingTilesToPlay -= StartEndOfGameCheck;
    }

    private void StartEndOfGameCheck()
    {
        tilePlayerFinishedPlaying = true;
    }

    private void Update()
    {
        if (!tilePlayerFinishedPlaying)
            return;

        //Units on map
        if (UnitSpawningManager.Instance.SpawnedUnits.NotNull().Count() > 0)
        {
            return;
        }

        if(unitsPassed >= RequiredNumberOfUnitsToPass)
        {
            Direction? opDirection = PathPicker.Instance.PickedDirection.HasValue ? (Direction)(-(int)PathPicker.Instance.PickedDirection) : null;
            MonsterSpawnManager.Instance.StartHorde(20, 20f, true, opDirection);
        }
        else
        {
            GameManager.Instance.EndGame();
        }
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
    }

    
}
