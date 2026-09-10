using GameManagerCalls;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>, ISaveLoad, IOnSave, IOnLoadAfterMapPointSceneLoad, IOnSetupNewGame, IOnClear
{
    public string SaveFileName => "ResourceManagerSave";

    public GameObject coin1;
    public GameObject coin5;
    public GameObject coin10;

    [SerializeField]
    private int startGold = 100;

    public int Gold { private set; get; }

    public int UpgradePoints {  private set; get; }

    [SerializeField]
    private LayerMask resourceLayerMask;

    public delegate void OnGoldChange(int currentGold);
    public OnGoldChange onGoldChange;

    public readonly List<GameObject> Coins = new List<GameObject>();

    private void OnEnable()
    {
        onGoldChange?.Invoke(Gold);
    }

    public void Clear()
    {
        foreach(GameObject coin in Coins)
        {
            Destroy(coin);
        }
        Coins.Clear();
    }


    public void OnSetupNewGame()
    {
        Gold = startGold;

        onGoldChange?.Invoke(Gold);
    }

    public void ModifyGold(int amount)
    {
        Gold = Mathf.Max(Gold + amount, 0);

        onGoldChange?.Invoke(Gold);
    }

    public void Save()
    {
        SaveManager.IfFileExistsDelete(SaveFileName);
        SaveData saveData = new SaveData(Gold, UpgradePoints);
        SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
        SaveManager.SaveOver(saveProfile);
    }

    public void LoadAfterMapPointSceneLoad()
    {
        if(SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
        {
            SaveData saveData = outSaveProfile.saveData;

            Gold = saveData.Gold;
            UpgradePoints = saveData.UpgradePoints;

            onGoldChange?.Invoke(Gold);
        }
    }

    [Serializable]
    private class SaveData : SaveProfileData
    {
        public int Gold;
        public int UpgradePoints;
        public SaveData(int Gold, int UpgradePoints)
        {
            this.Gold = Gold;
            this.UpgradePoints = UpgradePoints;
        }
    }
}
