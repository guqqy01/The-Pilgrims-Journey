using System.Collections.Generic;
using UnityEngine;
using GlobalMap;
using System.Linq;
using static UnityEngine.ParticleSystem;
using System;
using Units;
using Equipment;
using GameManagerCalls;
using LocalMap;
using Tiles;

public class RewardManager : Singleton<RewardManager>, ISaveLoad, IOnSave, IOnLoad, IOnAfterCreateLocalMap
{
    public string SaveFileName => $"RewardManagerSave";

    // Direction -> (Reward -> amount)
    public SerializableDictionary<Direction, SerializableDictionary<Reward, int>> LevelRewards { private set; get; } = new SerializableDictionary<Direction, SerializableDictionary<Reward, int>>();

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

    // keep name for serialization compatibility; consider renaming later
    public bool TilesOrEquipmentAvailiable = false;

    public Action<SerializableDictionary<Direction, SerializableDictionary<Reward, int>>> OnShowRewards;

    public Action<List<IDescriptable>> OnGiveEquipmentReward;
    public Action<TileSO, List<UnitSO>> OnGiveTileReward;
    public void Clear()
    {
        LevelRewards.Clear();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnShowRewards = null;
        OnGiveEquipmentReward = null;
        OnGiveTileReward = null;
    }

    public void GiveRewards()
    {
        var mapGen = GlobalMapGenerator.Instance;
        if (mapGen == null)
        {
            Debug.LogWarning("GiveRewards: GlobalMapGenerator.Instance is null.");
            return;
        }

        if (mapGen.CurrentMapPoint == null)
        {
            Debug.LogWarning("GiveRewards: CurrentMapPoint is null.");
            return;
        }

        UnityEngine.Random.InitState(mapGen.CurrentMapPoint.Seed);

        var prizeEquipment = new List<List<EquipmentSO>>();
        var prizeTiles = new List<TileAndUnit>();

        if (PathPicker.Instance == null)
        {
            Debug.LogWarning("GiveRewards: PathPicker.Instance is null.");
            return;
        }

        if (!Enum.IsDefined(typeof(Direction), PathPicker.Instance.PickedDirection))
        {
            Debug.LogWarning("GiveRewards: PickedDirection is not a valid Direction value.");
            return;
        }

        Direction pickedDirection = PathPicker.Instance.PickedDirection.Value;

        if (!LevelRewards.TryGetValue(pickedDirection, out var rewardsForDirection) || rewardsForDirection == null)
        {
            Debug.LogWarning($"GiveRewards: No rewards configured for direction {pickedDirection}.");
            TilesOrEquipmentAvailiable = false;
            return;
        }

        foreach (KeyValuePair<Reward, int> reward in rewardsForDirection)
        {
            switch (reward.Key)
            {
                case Reward.Gold:
                    ResourceManager.Instance?.ModifyGold(reward.Value);
                    break;
                case Reward.Tile:
                    for (int i = 0; i < reward.Value; i++)
                    {
                        var tile = GameManager.Instance?.Tiles?.GetRandom();

                        List<UnitSO> units = UnitManager.Instance.unlockedUnits.GetRandomAmount(2);

                        OnGiveTileReward?.Invoke(tile, units);
                    }
                    break;
                case Reward.Equipment:
                    for (int i = 0; i < reward.Value; i++)
                    {
                        List<EquipmentSO> availableEquipment = GameManager.Instance?.AvaliableEquipment;
                        if (availableEquipment == null || availableEquipment.Count == 0)
                            continue;

                        var equipmentList = new List<EquipmentSO>();
                        for (int j = 0; j < 3; j++)
                        {
                            int breaker = 10;
                            EquipmentSO equip = availableEquipment.GetRandom();
                            while (breaker > 0 && equipmentList.Contains(equip))
                            {
                                equip = availableEquipment.GetRandom();
                                breaker--;
                            }
                            if (equip != null)
                                equipmentList.Add(equip);
                        }

                        OnGiveEquipmentReward?.Invoke(equipmentList.Cast<IDescriptable>().ToList());
                    }
                    break;
            }
        }
    }

    public void SetupRewards()
    {
        LevelRewards.Clear();

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("SetupRewards: GameManager.Instance is null.");
            return;
        }

        int level = GameManager.Instance.Level;

        float max = 0f;

        var localMapGen = LocalMapGenerator.Instance;
        if (localMapGen == null)
        {
            Debug.LogWarning("SetupRewards: LocalMapGenerator.Instance is null.");
            return;
        }

        var pathway = localMapGen.pathway;
        if (pathway == null)
        {
            Debug.LogWarning("SetupRewards: pathway is null.");
            return;
        }

        var targetDirections = localMapGen.TargetDirections;
        if (targetDirections == null || targetDirections.Count == 0)
        {
            Debug.LogWarning("SetupRewards: No target directions found.");
            return;
        }

        foreach (Direction direction in targetDirections)
        {
            if (pathway.StepPaths.TryGetValue(direction, out var list) && list != null)
            {
                if (list.Count > max)
                    max = list.Count;
            }
        }

        if (max <= 0)
            max = 1f;

        foreach (Direction direction in targetDirections)
        {
            var rewards = new SerializableDictionary<Reward, int>();

            float eval = 0f;
            try
            {
                eval = rewardWorth.Evaluate(level, UnityEngine.Random.value);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"SetupRewards: failed to evaluate rewardWorth: {ex.Message}");
            }

            int currentRewardWorth = Mathf.CeilToInt(eval * (pathway.StepPaths.TryGetValue(direction, out var dlist) && dlist != null ? dlist.Count : 1) / max);

            int worth = 0;
            int breaker = 200;

            while (worth < currentRewardWorth && breaker > 0)
            {
                int remaining = currentRewardWorth - worth;

                if (remaining >= tileWorth || remaining >= equipmentWorth)
                {
                    var random = new WeightedChanceList<Reward>();

                    bool containsTileOrEquipment = rewards.ContainsKey(Reward.Tile) || rewards.ContainsKey(Reward.Equipment);

                    if (containsTileOrEquipment)
                    {
                        rewards.Add(Reward.Gold, remaining);
                        worth += remaining;
                        continue;
                    }

                    random.Add(Reward.Tile, containsTileOrEquipment ? 0 : 1);
                    random.Add(Reward.Equipment, containsTileOrEquipment ? 0 : 1);

                    switch (random.GetRandom())
                    {
                        case Reward.Tile:
                            rewards.Add(Reward.Tile, 1);
                            worth += tileWorth;
                            break;

                        case Reward.Equipment:
                            rewards.Add(Reward.Equipment, 1);
                            worth += equipmentWorth;
                            break;
                    }
                }
                else
                {
                    rewards.Add(Reward.Gold, remaining);
                    worth += remaining;
                }

                breaker--;
            }

            if (breaker <= 0)
                Debug.LogWarning($"SetupRewards: breaker reached limit while filling rewards for direction {direction}.");

            LevelRewards.Add(direction, rewards);
        }

        OnShowRewards?.Invoke(LevelRewards);
    }

    public void Save()
    {
        var saveData = new SaveData() { LevelRewards = LevelRewards };
        var saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
        SaveManager.SaveOver(saveProfile);
    }

    public void Load()
    {
        if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
        {
            var saveData = outSaveProfile.saveData;

            if (saveData.LevelRewards == null)
            {
                LevelRewards = new SerializableDictionary<Direction, SerializableDictionary<Reward, int>>();
            }
            else
            {
                LevelRewards = saveData.LevelRewards;
                OnShowRewards?.Invoke(LevelRewards);
            }
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
    private class SaveData : SaveProfileData
    {
        public SerializableDictionary<Direction, SerializableDictionary<Reward, int>> LevelRewards;
    }
}

