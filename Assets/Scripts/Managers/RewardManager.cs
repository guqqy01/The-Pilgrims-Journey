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

    public static RewardManager Instance { get; private set; }

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

    // Direction -> (Reward -> amount)
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

    // keep name for serialization compatibility; consider renaming later
    public bool TilesOrEquipmentAvailiable = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != this)
        {
            // prefer to destroy duplicate component, preserve GameObject
            Destroy(this);
        }
    }

    public void Clear()
    {
        LevelRewards.Clear();
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

        Direction pickedDirection = (Direction)PathPicker.Instance.PickedDirection;

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
                        var unit = UnitManager.Instance?.unlockedUnits?.GetRandom();
                        if (tile != null && unit != null)
                            prizeTiles.Add(new TileAndUnit(tile, unit));
                    }
                    break;
                case Reward.Equipment:
                    for (int i = 0; i < reward.Value; i++)
                    {
                        int numOfEquipment = UnityEngine.Random.Range(1, 4);
                        List<EquipmentSO> availableEquipment = GameManager.Instance?.AvaliableEquipment;
                        if (availableEquipment == null || availableEquipment.Count == 0)
                            continue;

                        var equipmentList = new List<EquipmentSO>();
                        for (int j = 0; j < numOfEquipment; j++)
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

                        if (equipmentList.Count > 0)
                            prizeEquipment.Add(equipmentList);
                    }
                    break;
            }
        }

        TilesOrEquipmentAvailiable = prizeEquipment.Count != 0 || prizeTiles.Count != 0;
        if (TilesOrEquipmentAvailiable)
        {
            PrizeWindow?.LoadPrize(prizeEquipment, prizeTiles);
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
                                rewards[Reward.Gold] += goldAmountPerReward;
                            else
                                rewards.Add(Reward.Gold, goldAmountPerReward);

                            worth += goldAmountPerReward;
                            break;

                        case Reward.Tile:
                            if (rewards.ContainsKey(Reward.Tile))
                                rewards[Reward.Tile] += 1;
                            else
                                rewards.Add(Reward.Tile, 1);

                            worth += tileWorth;
                            break;

                        case Reward.Equipment:
                            if (rewards.ContainsKey(Reward.Equipment))
                                rewards[Reward.Equipment] += 1;
                            else
                                rewards.Add(Reward.Equipment, 1);

                            worth += equipmentWorth;
                            break;
                    }
                }
                else
                {
                    if (rewards.ContainsKey(Reward.Gold))
                        rewards[Reward.Gold] += remaining;
                    else
                        rewards.Add(Reward.Gold, remaining);

                    worth += remaining;
                }

                breaker--;
            }

            if (breaker <= 0)
                Debug.LogWarning($"SetupRewards: breaker reached limit while filling rewards for direction {direction}.");

            LevelRewards.Add(direction, rewards);
        }

        SignUI?.Setup(LevelRewards.Keys.ToList());
    }

    public void Save()
    {
        var saveData = new SaveData() { LevelRewards = LevelRewards };
        var saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
        SaveManager.SaveOver(saveProfile);

        // wait for PrizeWindow instance then save it; helper handles null internally
        ExtensionsAndHelper.WaitForObjectNotNullThenPerformAction(PrizeWindow.Instance, () => PrizeWindow?.Save());
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
                SignUI?.Setup(LevelRewards.Keys.ToList());
            }
            PrizeWindow?.Load();
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


