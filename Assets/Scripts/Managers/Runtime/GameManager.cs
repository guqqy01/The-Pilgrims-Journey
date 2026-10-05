using Equipment;
using GameManagerCalls;
using GlobalMap;
using Helper.Wait;
using Heros;
using LocalMap;
using System;
using System.Collections.Generic;
using Tiles;
using TPJ.Gameplay;
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
        InterfaceCaller.CallInterfaces<IOnClear>(x => x.Clear());
        

        pendingHeroSO = heroSO;
        currentSeed = Random.Range(int.MinValue, int.MaxValue);

        GenerateGlobalMap(currentSeed);
    }

    private void GenerateGlobalMap(int seed)
    {
        GlobalMapGenerator.Instance.GenerateMap(seed);

        GlobalMapGenerator.Instance.CurrentMapPoint.LoadScene(SetupGame);
    }

    private void SetupPhaseOne(CardGameManager manager)
    {
        manager.ResetForNewGame();
        manager.PlayerDeckHolder.SetHeroCard(pendingHeroSO);

        PlayerCollection.IfReady(collection =>
        {
            foreach (ICardable cardable in collection.CreateCardables())
                manager.PlayerDeckHolder.AddCardToDeck(cardable.CreateCard());
        });

        CardBase attackCard = pendingHeroSO.startingAttack.CreateCard();
        manager.PlayerDeckHolder.AddCardToDeck(attackCard);
        manager.BeginGame();
    }

    private void SetupGame()
    {
        Delete();

        Level = 1;

        InterfaceCaller.CallInterfaces<IOnSetupNewGame>(x => x.OnSetupNewGame());

        HeroManager.WhenReady(manager => manager.SelectHero(pendingHeroSO));
        CardGameManager.WhenReady(SetupPhaseOne);
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


    public void CompleteLevel()
    {
        InterfaceCaller.CallInterfaces<IOnCompletedLevel>(x => x.OnLevelCompleted());

        Level++;

        Save();
    }

    public void LoadCurrentMapPoint(Action action = null, bool fromContinueingGame = false)
    {
        GlobalMapPointType type = GlobalMapGenerator.Instance.CurrentMapPoint.Type;

        if(fromContinueingGame)
            action += () => InterfaceCaller.CallInterfaces<IOnLoadAfterMapPointSceneLoad>(x => x.LoadAfterMapPointSceneLoad());

        switch (type)
        {
            case GlobalMapPointType.Normal:
                action +=  () => CardGameManager.WhenReady(SetupPhaseOne);
                break;
            case GlobalMapPointType.Random:
                action += GenerateRandomEvent;
                break;
            case GlobalMapPointType.Relic:
                action += () => CardGameManager.WhenReady(SetupPhaseOne);
                break;
            case GlobalMapPointType.Shop:
                if (!fromContinueingGame)
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
        InterfaceCaller.CallInterfaces<IOnGenerateNewRandomEvent>(x => x.OnGenerateNewRandomEvent());
    }

    public void CreateLocalMap()
    {
        InterfaceCaller.CallInterfaces<IOnBeforeCreateLocalMap>(x => x.OnBeforeCreateLocalMap());
        InterfaceCaller.CallInterfaces<IOnCreateLocalMap>(x => x.OnCreateLocalMap());
        InterfaceCaller.CallInterfaces<IOnAfterCreateLocalMap>(x => x.AfterCreateLocalMap());

        Save();
    }

    public void EndGame(string reason)
    {
        InterfaceCaller.CallInterfaces<IOnGameEnd>(x => x.OnGameEnd(reason));

        Delete();
    }

    private void Delete()
    {
        SaveManager.DeleteAllSaves();
        InterfaceCaller.CallInterfaces<IOnDelete>(x => x.Delete());
    }

    public void Save()
    {
        SaveManager.IfFileExistsDelete(SaveFileName);
        SaveData saveData = new SaveData(Level, currentSeed);
        SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
        SaveManager.SaveOver(saveProfile);

        InterfaceCaller.CallInterfaces<IOnSave>(x => x.Save());
    }

    public void Load()
    {
        if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
        {
            SaveData saveData = outSaveProfile.saveData;

            Level = saveData.Level;
            currentSeed = saveData.Seed;

            Debug.Log("Call Load");
            InterfaceCaller.CallInterfaces<IOnLoad>(x => x.Load());
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
