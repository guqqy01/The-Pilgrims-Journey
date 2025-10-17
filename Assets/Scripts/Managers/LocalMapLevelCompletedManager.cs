using GameManagerCalls;
using Map;
using Unity.VisualScripting;
using UnityEngine;
using static GameManager;
using System.Collections.Generic;
using System.Linq;
using Tiles;
public class LocalMapLevelCompletedManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot, IOnBeforeCreateLocalMap
{
    public static LocalMapLevelCompletedManager Instance { private set; get; }

    private int unitsPassed = 0;

    [SerializeField]
    private AnimationCurve requiredUnits;

    public bool CurrentLevelCompleted { private set; get; } = false;

    public int RequiredNumberOfUnitsToPass => Mathf.CeilToInt(requiredUnits.Evaluate(GameManager.Instance.Level));

    private bool beginCheckingGameEndCondition;


    public delegate void OnUnitPassed(int totalPassed, int requiredToPass);
    public static OnUnitPassed onUnitPassed;

    public void InitiateSingleton()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForObjectHelper.WaitForObject(() => TilePlayerManager.Instance, SubscribeToEndOfGameCheck, 1f));
    }

    private void SubscribeToEndOfGameCheck()
    {
        TilePlayerManager.Instance.noRemainingTilesToPlay += StartEndOfGameCheck;
    }

    private void OnDestroy()
    {
        if (TilePlayerManager.Instance != null)
            TilePlayerManager.Instance.noRemainingTilesToPlay -= StartEndOfGameCheck;
    }

    private void StartEndOfGameCheck()
    {
        beginCheckingGameEndCondition = true;
    }

    private void Update()
    {
        if (!beginCheckingGameEndCondition)
            return;

        //Units on map
        if (SpawningManager.Instance.SpawnedUnits.NotNull().Count() > 0)
        {
            return;
        }

        GameManager.Instance.EndGame();
    }

    public void UnitPassed()
    {
        unitsPassed++;
        onUnitPassed?.Invoke(unitsPassed, RequiredNumberOfUnitsToPass);

        if (unitsPassed >= RequiredNumberOfUnitsToPass)
        {
            CompleteLevel();
        }
    }

    public void CompleteLevel()
    {
        beginCheckingGameEndCondition = false;
        CurrentLevelCompleted = true;
        if (GlobalMapGenerator.Instance.CurrentMapPoint.Type == GlobalMapPointType.Normal)
            RewardManager.Instance.GiveRewards();

        InterfaceCaller.CallInterfaces<IOnCompleteLocalMapLevel>(x => x.OnCompleteLocalMapLevel());
    }

    public void OnBeforeCreateLocalMap()
    {
        unitsPassed = 0;
        CurrentLevelCompleted = false;
        beginCheckingGameEndCondition = false;
    }
}
