using Codice.CM.Common;
using Equipment;
using GameManagerCalls;
using GlobalMap;
using Heros;
using LocalMap;
using System;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour, ISaveLoad
{
    public static GameManager Instance { private set; get; }

    public LocalMapGenerationSettings LocalMapSettings { get; private set; }

    public int Level { private set; get; } = 1;

    public string SaveFileName => "GameManagerSave";

    public List<EquipmentSO> AvaliableEquipment;

    public WeightedChanceList<TileSO> Tiles;

    public bool LocalMapPlaying => LocalMapLevelCompletedManager.Instance.CurrentlyPlayingLocalLevel;

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

    #region Start New Journey

    private HeroSO pendingHeroSO;

    public void StartNewJourney(HeroSO heroSO)
    {
        GameCalls.Clear();
        
        pendingHeroSO = heroSO;
        currentSeed = Random.Range(int.MinValue, int.MaxValue);

        GlobalMapGenerator.Instance.GenerateMap(currentSeed);
        SetupGame();
    }

    private void SetupGame()
    {
        Delete();

        Level = 1;

        Action action = GameCalls.SetupNewGame;
        action += () => HeroManager.WhenReady(manager => manager.SelectHero(pendingHeroSO));
        LoadCurrentMapPoint(action, false);
    }

    #endregion

    #region Continue Journey

    public void ContinueJourney()
    {
        GameCalls.Clear();

        Load();

        LoadCurrentMapPoint(null, true);
    }

    #endregion


    public void CompleteLevel()
    {
        GameCalls.LevelCompleted();

        Level++;

        Save();
    }

    public void LoadCurrentMapPoint(Action action = null, bool fromContinueingGame = false)
    {
        GlobalMapPointType type = GlobalMapGenerator.Instance.CurrentMapPoint.Type;

        if(fromContinueingGame)
            action += GameCalls.LoadAfterMapPointSceneLoad;

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
                if (!fromContinueingGame)
                    action += SetupShop;
                break;
        }
        action += GameCalls.CreateCurrentMapPoint;

        GlobalMapGenerator.Instance.CurrentMapPoint.LoadScene(action);
    }

    void SetupShop()
    {
        ShopManager.Instance.SetupShop();
    }

    void GenerateRandomEvent()
    {
        GameCalls.GenerateNewRandomEvent();
    }

    public void CreateLocalMap()
    {
        GameCalls.BeforeCreateLocalMap();
        GameCalls.CreateLocalMap();
        GameCalls.AfterCreateLocalMap();

        Save();
    }

    public void EndGame(string reason)
    {
        GameCalls.GameEnd(reason);

        Delete();
    }

    private void Delete()
    {
        SaveManager.DeleteAllSaves();
        GameCalls.Delete();
    }

    public void Save()
    {
        SaveManager.IfFileExistsDelete(SaveFileName);
        SaveData saveData = new SaveData(Level, currentSeed);
        SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
        SaveManager.SaveOver(saveProfile);

        GameCalls.Save();
    }

    public void Load()
    {
        if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
        {
            SaveData saveData = outSaveProfile.saveData;

            Level = saveData.Level;
            currentSeed = saveData.Seed;

            GameCalls.Load();
        }
        else
        {
            Debug.LogWarning($"No save data found for {SaveFileName}");
            return;
        }
    }

    [Serializable]
    private class SaveData : SaveProfileData
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
