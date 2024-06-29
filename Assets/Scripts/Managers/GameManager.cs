using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Map;
using System.Linq;
using Unity.VisualScripting;
using Units;
using System;
using Equipment;
using Tiles;
using Random = UnityEngine.Random;
using GameManagerCalls;


public class GameManager : MonoBehaviour, ISaveLoad
{
    public static GameManager Instance { private set; get; }

    public LocalMapGenerationSettings LocalMapSettings { get; private set; }

    public int Level { private set; get; } = 1;

    private int unitsPassed = 0;

    [SerializeField]
    private AnimationCurve requiredUnits;
    public int RequiredNumberOfUnitsToPass => Mathf.CeilToInt(requiredUnits.Evaluate(Level));

    public string SaveFileName => "GameManagerSave";

    public List<EquipmentSO> AvaliableEquipment;

    public WeightedChanceList<TileSO> Tiles;

    private bool beginCheckingGameEndCondition;

    public delegate void OnLevelChange();
    public static OnLevelChange onCompleteLevel;

    public delegate void OnGameEnd();
    public static OnGameEnd onGameEnd;

    public delegate void OnUnitPassed(int totalPassed, int requiredToPass);
    public static OnUnitPassed onUnitPassed;

    public delegate void OnClear();
    public static OnClear onClear;

    public delegate void OnAfterCreateLocalMap();
    public static event OnAfterCreateLocalMap onAfterCreateLocalMap;

    public delegate void OnLoadLocalMap();
    public static event OnLoadLocalMap onCreateLocalMap;

    public delegate void OnBeforeCreateLocalMap();
    public static event OnBeforeCreateLocalMap onBeforeCreateLocalMap;

    public delegate void OnLoadCurrentMapPoint();
    public static event  OnLoadCurrentMapPoint onCreateCurrentMapPoint;

    public delegate void OnCompleteLocalMapLevel();
    public static event OnCompleteLocalMapLevel onCompleteLocalMapLevel;

    public delegate void OnSetup();
    public static OnSetup onSetup;

    public delegate void OnLoad();
    public static event OnLoad onLoad;

    public delegate void OnSave();
    public static event OnSave onSave;

    public delegate void OnDelete();
    public static event OnDelete onDelete;

    public bool LocalMapPlaying;

    private int currentSeed;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        if(TilePlayerManager.Instance != null)
        {
            TilePlayerManager.Instance.noRemainingTilesToPlay += () => beginCheckingGameEndCondition = true;
        }
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

        GameEnd();
    }

    #region Start New Journey

    public void StartNewJourney()
    {
        onClear?.Invoke();

        currentSeed = Random.Range(int.MinValue, int.MaxValue);
        GenerateGlobalMap(currentSeed);
    }

    private void GenerateGlobalMap(int seed)
    {
        GlobalMapGenerator.Instance.GenerateMap(seed);

        GlobalMapGenerator.Instance.CurrentMapPoint.LoadScene(SetupFirstLocalMap);
    }

    private void SetupFirstLocalMap()
    {
        Delete();
        onClear?.Invoke();
        Setup();

        CreateLocalMap();

        onCreateCurrentMapPoint?.Invoke();
    }
    
    private void Setup()
    {
        Level = 1;
        onSetup?.Invoke();

        List<ISetup> ISetupInterfaces = InterfaceFinder.FindInterfaces<ISetup>();

        foreach(ISetup setupCall in ISetupInterfaces)
        {
            setupCall.Setup();
        }
    } 



    #endregion

    #region Continue Journey

    public void ContinueJourney()
    {
        onClear?.Invoke();

        Load();

        GlobalMapGenerator.Instance.GenerateMap(currentSeed);

        CreateLocalMap();
    }

    #endregion

    public void UnitPassed()
    {
        unitsPassed++;
        onUnitPassed?.Invoke(unitsPassed, Mathf.CeilToInt(requiredUnits.Evaluate(Level)));

        if (unitsPassed >= requiredUnits.Evaluate(Level))
        {
            CompleteLevel();
        }

    }

    public void CompleteLevel()
    {
        beginCheckingGameEndCondition = false;

        if(GlobalMapGenerator.Instance.CurrentMapPoint.Type == GlobalMapPointType.Normal)
            RewardManager.Instance.GiveRewards();


        if (GlobalMapGenerator.Instance.CurrentMapPoint.Type == GlobalMapPointType.Normal ||
            GlobalMapGenerator.Instance.CurrentMapPoint.Type == GlobalMapPointType.Relic)
        {
            onCompleteLocalMapLevel?.Invoke();
        }

        onCompleteLevel?.Invoke();

        Level++;

        Save();
    }

    public void LoadCurrentMapPoint()
    {
        GlobalMapPointType type = GlobalMapGenerator.Instance.CurrentMapPoint.Type;

        Action action = null;

        switch (type)
        {
            case GlobalMapPointType.Normal:
                action += CreateLocalMap;
                break;
            case GlobalMapPointType.Random:
                action += GenerateRandomEvent;
                break;
            case GlobalMapPointType.Relic:
                action += CreateLocalMap;
                break;
            case GlobalMapPointType.Shop:
                break;
        }
        
        action += () => onCreateCurrentMapPoint?.Invoke();
        GlobalMapGenerator.Instance.CurrentMapPoint.LoadScene(action);
    }

    void GenerateRandomEvent()
    {
        RandomEventGenerator.Instance.GenerateRandomEvent();
    }

    private void CreateLocalMap()
    {
        unitsPassed = 0;
        
        onBeforeCreateLocalMap?.Invoke();
        onCreateLocalMap?.Invoke();
        onAfterCreateLocalMap?.Invoke();

        Save();
    }

    private void GameEnd()
    {
        beginCheckingGameEndCondition = false;
        onGameEnd?.Invoke();
        onClear?.Invoke();
        Delete();
    }

    private void Delete()
    {
        SaveManager.IfFileExistsDelete(SaveFileName);

        onDelete?.Invoke();
    }

    public void Save()
    {
        SaveManager.IfFileExistsDelete(SaveFileName);
        SaveData saveData = new SaveData(Level, currentSeed);
        SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
        SaveManager.Save(saveProfile);

        onSave?.Invoke();
    }

    public void Load()
    {
        SaveData saveData = SaveManager.Load<SaveData>(SaveFileName).saveData;

        Level = saveData.Level;
        currentSeed = saveData.Seed;

        onLoad?.Invoke();
    }

    [Serializable]
    private record SaveData : SaveProfileData
    {
        public int Level;
        public int Seed;


        public SaveData(int Level, int seed)
        {
            this.Seed = seed;
            this.Level = Level;
        }
    }

}


                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       

public enum Reward
{
    Gold = 1,
    Equipment = 100,
    Tile = 120,
}
