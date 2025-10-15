using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;
using System.Linq;
using static UnityEngine.ParticleSystem;
using System;
using Units;
using Equipment;
using UnityEngine.UI;
using GameManagerCalls;

public class RewardManager : MonoBehaviour, ISaveLoad, IOnSave, IOnLoad, IOnAfterCreateLocalMap
{
    public string SaveFileName => $"RewardManagerSave";

    public static RewardManager Instance { private set; get; }

    private SignUI signUI;
    public SignUI SignUI
    {
        get
        {
            if (signUI == null)
                signUI = SignUI.Instance;
            return signUI;
        }
    }

    public SerializableDictionary<Direction, SerializableDictionary<Reward, int>> LevelRewards { private set; get; } = new SerializableDictionary<Direction, SerializableDictionary<Reward, int>>();

    private PrizeWindow prizeWindow;
    public PrizeWindow PrizeWindow
    {
        get
        {
            if (prizeWindow == null)
                prizeWindow = PrizeWindow.Instance;
            return prizeWindow;
        }
    }

    

    [SerializeField]
    private MinMaxCurve rewardWorth;

    [Header("Rewards")]
    [SerializeField]
    private int goldAmountPerReward = 50;
    [SerializeField]
    private int tileWorth = 100;
    public int TileWorth => tileWorth;
    [SerializeField]
    private int equipmentWorth = 100;
    public int EquipmentWorth => equipmentWorth;
    [SerializeField]
    private int diamondWorth = 300;
    public int DiamondWorth => diamondWorth;

    [SerializeField]
    private float goldChanceIncreasePerEquipment = 5f;
    [SerializeField]
    private float goldChanceIncreasePerTile = 10f;
    [SerializeField]
    private float equipmentChanceIncreasePerTile = 5f;

    public bool TilesOrEquipmentAvailiable = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Clear()
    {
        LevelRewards.Clear();
    }

    public void GiveRewards()
    {
        UnityEngine.Random.InitState(GlobalMapGenerator.Instance.CurrentMapPoint.Seed);

        List<List<EquipmentSO>>  prizeEquipment = new List<List<EquipmentSO>>();
        List<TileAndUnit>  prizeTiles = new List<TileAndUnit>();

        Direction pickedDirection = (Direction)PathPicker.Instance.PickedDirection;

        foreach (KeyValuePair<Reward, int> reward in LevelRewards[pickedDirection])
        {
            switch (reward.Key)
            {
                case Reward.Gold:
                    ResourceManager.Instance.ModifyGold(reward.Value);
                    break;
                case Reward.Tile:
                    for (int i = 0; i < reward.Value; i++)
                    {
                        prizeTiles.Add(new TileAndUnit(GameManager.Instance.Tiles.GetRandom(), UnitManager.Instance.unlockedUnits.GetRandom()));
                    }
                    break;
                case Reward.Equipment:
                    for (int i = 0; i < reward.Value; i++)
                    {
                        int numOfEquipment = UnityEngine.Random.Range(1, 4);

                        List<EquipmentSO> avaliableEquipment = GameManager.Instance.AvaliableEquipment;

                        List<EquipmentSO> equipment = new List<EquipmentSO>();

                        for (int j = 0; j < numOfEquipment; j++)
                        {
                            int breaker = 10;

                            EquipmentSO equip = avaliableEquipment.GetRandom();

                            while (breaker > 0 && equipment.Contains(equip))
                            {
                                equip = avaliableEquipment.GetRandom();
                                breaker--;
                            }

                            equipment.Add(equip);
                        }

                        prizeEquipment.Add(equipment);
                    }
                    
                    break;
            }
        }

        TilesOrEquipmentAvailiable = false;
        if (prizeEquipment.Count != 0 || prizeTiles.Count != 0)
        {
            TilesOrEquipmentAvailiable = true;
            PrizeWindow.LoadPrize(prizeEquipment, prizeTiles);
        }
    }

    public void SetupRewards()
    {
        LevelRewards.Clear();

        int Level = GameManager.Instance.Level;

        float max = 0;

        Pathway pathway = LocalMapGenerator.Instance.pathway;

        foreach (Direction direction in LocalMapGenerator.Instance.TargetDirections)
        {
            if (pathway.StepPaths[direction].Count > max)
            {
                max = pathway.StepPaths[direction].Count;
            }
        }

        foreach (Direction direction in LocalMapGenerator.Instance.TargetDirections)
        {
            SerializableDictionary<Reward, int> rewards = new SerializableDictionary<Reward, int>();

            int currentRewardWorth = Mathf.CeilToInt(rewardWorth.Evaluate(Level, UnityEngine.Random.value) * pathway.StepPaths[direction].Count / max);

            int worth = 0;

            int breaker = 200;

            while (worth < currentRewardWorth && breaker > 0)
            {
                int remaining = currentRewardWorth - worth;

                if (remaining >= tileWorth || remaining >= equipmentWorth)
                {
                    WeightedChanceList<Reward> random = new WeightedChanceList<Reward>();

                    random.Add(Reward.Gold, 1);
                    if (rewards.ContainsKey(Reward.Equipment))
                        random[Reward.Gold] += Mathf.RoundToInt(rewards[Reward.Equipment] * goldChanceIncreasePerEquipment);
                    if (rewards.ContainsKey(Reward.Tile))
                        random[Reward.Gold] += Mathf.RoundToInt(rewards[Reward.Tile] * goldChanceIncreasePerTile);

                    random.Add(Reward.Tile, 1);

                    random.Add(Reward.Equipment, 1);
                    if (rewards.ContainsKey(Reward.Tile))
                        random[Reward.Equipment] += Mathf.RoundToInt(rewards[Reward.Tile] * equipmentChanceIncreasePerTile);

                    switch (random.GetRandom())
                    {
                        case Reward.Gold:
                            if (rewards.ContainsKey(Reward.Gold))
                            {
                                rewards[Reward.Gold] += goldAmountPerReward;
                            }
                            else
                            {
                                rewards.Add(Reward.Gold, goldAmountPerReward);
                            }

                            worth += goldAmountPerReward;
                            break;
                        case Reward.Tile:
                            if (rewards.ContainsKey(Reward.Tile))
                            {
                                rewards[Reward.Tile] += 1;
                            }
                            else
                            {
                                rewards.Add(Reward.Tile, 1);
                            }

                            worth += tileWorth;
                            break;
                        case Reward.Equipment:
                            if (rewards.ContainsKey(Reward.Equipment))
                            {
                                rewards[Reward.Equipment] += 1;
                            }
                            else
                            {
                                rewards.Add(Reward.Equipment, 1);
                            }
                            worth += equipmentWorth;
                            break;
                    }
                }
                else
                {
                    if (rewards.ContainsKey(Reward.Gold))
                    {
                        rewards[Reward.Gold] += remaining;
                    }
                    else
                    {
                        rewards.Add(Reward.Gold, remaining);
                    }
                    worth += remaining;
                }

                breaker--;
            }

            LevelRewards.Add(direction, rewards);
        }

        SignUI.Instance.Setup(LevelRewards.Keys.ToList());
    }

    public void Save()
    {
        SaveData saveData = new SaveData() { LevelRewards = LevelRewards };
        SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
        SaveManager.SaveOver(saveProfile);
        ExtensionsAndHelper.WaitForObjectNotNullThenPerformAction(PrizeWindow.Instance, () => PrizeWindow.Save());
    }

    public void Load()
    {
        if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
        {
            SaveData saveData = outSaveProfile.saveData;

            if (saveData.LevelRewards == null)
            {
                LevelRewards = new SerializableDictionary<Direction, SerializableDictionary<Reward, int>>();
            }
            else
            {
                LevelRewards = saveData.LevelRewards;
                SignUI.Instance.Setup(LevelRewards.Keys.ToList());
            }
            PrizeWindow.Load();
        }
        else
        {
            Debug.LogWarning($"No save data found for {SaveFileName}");
            return;
        }
    }

    public void AfterCreateLocalMap()
    {
        SetupRewards();
    }

    [Serializable]
    private record SaveData : SaveProfileData
    {
        public SerializableDictionary<Direction, SerializableDictionary<Reward, int>> LevelRewards;
    }
}


