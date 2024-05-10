using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Map;

public class SignDirectionUI : MonoBehaviour
{
    private GameManager gameManager;

    private GameManager GameManager
    {
        get
        {
            if (gameManager == null)
                gameManager = GameManager.Instance;

            return gameManager;
        }
    }

    public static SignDirectionUI Instance;

    [SerializeField]
    private RewardItemUI rewardItemUIPrefab;

    [SerializeField]
    private Transform rewardItemsHolder;

    private List<RewardItemUI> rewardItemUIs = new List<RewardItemUI>();

    [SerializeField]
    private Text directionText;

    private Direction currentDirection;

    [SerializeField]
    private Sprite diamondSprite;
    [SerializeField]
    private Sprite goldSprite;
    [SerializeField]
    private Sprite tileSprite;
    [SerializeField]
    private Sprite equipmentSprite;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Setup(Direction direction)
    {
        currentDirection = direction;

        directionText.text = direction.ToString();

        foreach (RewardItemUI item in rewardItemUIs)
        {
            Destroy(item.gameObject);
        }

        rewardItemUIs.Clear();

        Dictionary<Reward, int> rewards = GameManager.LevelRewards[direction];

        foreach (KeyValuePair<Reward, int> reward in rewards)
        {
            RewardItemUI item = Instantiate(rewardItemUIPrefab, rewardItemsHolder);

            Sprite sprite = null;

            switch (reward.Key)
            {
                case Reward.Diamond:
                    sprite = diamondSprite;
                    break;
                case Reward.Gold:
                    sprite = goldSprite;
                    break;
                case Reward.Tile:
                    sprite = tileSprite;
                    break;
                case Reward.Equipment:
                    sprite = equipmentSprite;
                    break;
            }

            item.Init(sprite, reward.Value.ToString());

            rewardItemUIs.Add(item);
        }
    }

    public void OnPickDirectionButton()
    {
        PathPicker.Instance.PickDirection(currentDirection);
    }
}
