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

    public delegate void OnUnitPassed(int totalPassed, int requiredToPass);
    public static OnUnitPassed onUnitPassed;


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
            TilePlayerManager.Instance.noRemainingTilesToPlay += StartCheck;
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

    private void OnDestroy()
    {
        TilePlayerManager.Instance.noRemainingTilesToPlay -= StartCheck;
    }

    private void StartCheck()
    {
        beginCheckingGameEndCondition = true;
    }

    #region Start New Journey

    public void StartNewJourney()
    {
        InterfaceCaller.CallInterfaces<IOnClear>(x => x.Clear());

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
        InterfaceCaller.CallInterfaces<IOnClear>(x => x.Clear());

        Setup();

        CreateLocalMap();

        InterfaceCaller.CallInterfaces<IOnCreateCurrentMapPoint>(x => x.OnCreateCurrentMapPoint());
    }
    
    private void Setup()
    {
        Level = 1;

        InterfaceCaller.CallInterfaces<IOnSetup>(x => x.SetupNewGame());
    } 



    #endregion

    #region Continue Journey

    public void ContinueJourney()
    {
        InterfaceCaller.CallInterfaces<IOnClear>(x => x.Clear());

        Load();

        LoadCurrentMapPoint(null, true);
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
            InterfaceCaller.CallInterfaces<IOnCompleteLocalMapLevel>(x => x.OnCompleteLocalMapLevel());
        }

        InterfaceCaller.CallInterfaces<IOnCompletedLevel>(x => x.OnLevelCompleted());

        Level++;

        Save();
    }

    public void LoadCurrentMapPoint(Action action = null, bool fromContinueingGame = false)
    {
        GlobalMapPointType type = GlobalMapGenerator.Instance.CurrentMapPoint.Type;

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
                if (fromContinueingGame)
                    action += () => InterfaceCaller.CallInterfaces<IOnLoadAfterMapPointSceneLoad>(x => x.LoadAfterMapPointSceneLoad());
                else
                    action += SetupShop;
                break;
        }
        action += () => InterfaceCaller.CallInterfaces<IOnCreateCurrentMapPoint>(x => x.OnCreateCurrentMapPoint());

        GlobalMapGenerator.Instance.CurrentMapPoint.LoadScene(action);
    }

    void SetupShop()
    {
        ShopManager.Instance.SetupShop();
    }

    void GenerateRandomEvent()
    {
        RandomEventGenerator.Instance.GenerateRandomEvent();
    }

    private void CreateLocalMap()
    {
        unitsPassed = 0;

        InterfaceCaller.CallInterfaces<IOnBeforeCreateLocalMap>(x => x.OnBeforeCreateLocalMap());
        InterfaceCaller.CallInterfaces<IOnCreateLocalMap>(x => x.OnCreateLocalMap());
        InterfaceCaller.CallInterfaces<IOnAfterCreateLocalMap>(x => x.AfterCreateLocalMap());

        Save();
    }

    private void GameEnd()
    {
        beginCheckingGameEndCondition = false;

        InterfaceCaller.CallInterfaces<IOnGameEnd>(x => x.OnGameEnd());
        InterfaceCaller.CallInterfaces<IOnClear>(x => x.Clear());

        Delete();
    }

    private void Delete()
    {
        SaveManager.IfFileExistsDelete(SaveFileName);

        InterfaceCaller.CallInterfaces<IOnDelete>(x => x.Delete());
    }

    public void Save()
    {
        SaveManager.IfFileExistsDelete(SaveFileName);
        SaveData saveData = new SaveData(Level, currentSeed);
        SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
        SaveManager.Save(saveProfile);

        InterfaceCaller.CallInterfaces<IOnSave>(x => x.Save());
    }

    public void Load()
    {
        if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
        {
            SaveData saveData = outSaveProfile.saveData;

            Level = saveData.Level;
            currentSeed = saveData.Seed;

            InterfaceCaller.CallInterfaces<IOnLoad>(x => x.Load());
        }
        else
        {
            Debug.LogWarning($"No save data found for {SaveFileName}");
            return;
        }
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
