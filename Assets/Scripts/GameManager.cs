using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Map;
using static UnityEngine.ParticleSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public LocalMapGenerationSettings LocalMapSettings { get; private set; }

    public int Level = 1;

    private int unitsPassed = 0;

    [SerializeField]
    private AnimationCurve requiredUnits;

    [SerializeField]
    private MinMaxCurve rewardWorth;

    [SerializeField]
    private Text levelText;
    [SerializeField]
    private Text requiredUnitsText;

    public Dictionary<Direction, Dictionary<Reward, int>> LevelRewards { private set; get; } = new Dictionary<Direction, Dictionary<Reward, int>>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        LocalMapGenerator.Instance.CreateMap();
        PrizeWindow.instance.gameObject.SetActive(true);

        levelText.text = "Level " + Level;
        requiredUnitsText.text = unitsPassed + "/" + Mathf.CeilToInt(requiredUnits.Evaluate(Level));
        LoadRewards();
    }

    public void SetLocalMapObject(LocalMapGenerationSettings settings)
    {
        LocalMapSettings = settings;
    }

    public void SetupLocalMap()
    {
        LocalMapGenerator.Instance.generationSettings = LocalMapSettings;
        LocalMapGenerator.Instance.CreateMap();
    }

    public void UnitPassed()
    {
        unitsPassed++;

        if (unitsPassed >= requiredUnits.Evaluate(Level))
            CompleteLevel();

        requiredUnitsText.text = unitsPassed + "/" + Mathf.CeilToInt(requiredUnits.Evaluate(Level));
    }

    public void LoadRewards()
    {
        LevelRewards.Clear();

        foreach(Direction direction in LocalMapGenerator.Instance.targetDirections)
        {
            Dictionary<Reward, int> rewards = new Dictionary<Reward, int>();

            int currentRewardWorth = Mathf.CeilToInt(rewardWorth.Evaluate(Level, Random.value));

            int worth = 0;

            if (Level % 5 == 0)
            {
                rewards.Add(Reward.Diamond, 1);
                worth += (int)Reward.Diamond;
            }

            int breaker = 20;

            while (worth < currentRewardWorth && breaker > 0)
            {
                int remaining = currentRewardWorth - worth;

                if(remaining >= 100)
                {
                    int randomint = Random.Range(0, 3);

                    switch (randomint)
                    {
                        case 0:
                            if (rewards.ContainsKey(Reward.Gold))
                            {
                                rewards[Reward.Gold] += 100;
                            }
                            else
                            {
                                rewards.Add(Reward.Gold, 100);
                            }

                            worth += 100;
                            break;
                        case 1:
                            if (rewards.ContainsKey(Reward.Tile))
                            {
                                rewards[Reward.Tile] += 1;
                            }
                            else
                            {
                                rewards.Add(Reward.Tile, 1);
                            }

                            worth += (int)Reward.Tile;
                            break;
                        case 2:
                            if (rewards.ContainsKey(Reward.Equipment))
                            {
                                rewards[Reward.Equipment] += 1;
                            }
                            else
                            {
                                rewards.Add(Reward.Equipment, 1);
                            }
                            worth += (int)Reward.Equipment;
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
    }

    private void CompleteLevel()
    {
        TilePlayer.Instance.StopAndReset();

        int numOfEquipment = 0;
        int numOfTiles = 0;

        foreach(KeyValuePair<Reward, int> reward in LevelRewards[(Direction)PathPicker.Instance.PickedDirection])
        {
            switch (reward.Key)
            {
                case Reward.Gold:
                    ResourceManager.Instance.ModifyGold(reward.Value);
                    break;
                case Reward.Diamond:
                    ResourceManager.Instance.ModifyDiamonds(reward.Value);
                    break;
                case Reward.Tile:
                    numOfTiles = reward.Value;
                    break;
                case Reward.Equipment:
                    numOfEquipment = reward.Value;
                    break;
            }
        }

        PrizeWindow.instance.gameObject.SetActive(true);
        PrizeWindow.instance.LoadPrize(numOfEquipment, numOfTiles);


        Level++;

        unitsPassed = 0;

        LocalMapGenerator.Instance.CreateMap();

        levelText.text = "Level " + Level;

        LoadRewards();

        SignDirectionUI.Instance.gameObject.SetActive(false);
    }
}

public enum Reward
{
    Gold = 1,
    Equipment = 100,
    Tile = 120,
    Diamond = 200,
}
