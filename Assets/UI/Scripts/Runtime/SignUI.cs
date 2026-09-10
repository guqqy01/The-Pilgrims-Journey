using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalMap;
using System.Linq;
using TMPro;

namespace LocalMap.UI
{
    public class SignUI : Singleton<SignUI>
    {
        [SerializeField]
        private RewardItemUI rewardItemUIPrefab;

        [SerializeField]
        private Transform rewardItemsHolder;

        private readonly List<RewardItemUI> rewardItemUIs = new List<RewardItemUI>();

        [SerializeField]
        private TextMeshProUGUI directionText;

        private Direction currentDirection;

        [SerializeField]
        private Sprite goldSprite;
        [SerializeField]
        private Sprite tileSprite;
        [SerializeField]
        private Sprite equipmentSprite;

        [SerializeField]
        private Image northArrow;
        [SerializeField]
        private Image southArrow;
        [SerializeField]
        private Image eastArrow;
        [SerializeField]
        private Image westArrow;

        [SerializeField]
        private Sprite normalArrowSprite;
        [SerializeField]
        private Sprite hightlightedArrowSprite;

        private void Start()
        {
            PathPicker.WhenReady(SubscribeToPathPicker);
            RewardManager.WhenReady(SubscribeToRewardManager);
        }

        private void SubscribeToRewardManager(RewardManager rewardManager)
        {
            rewardManager.OnShowRewards += Setup;
        }
        private void UnsubscribeFromRewardManager(RewardManager rewardManager)
        {
            rewardManager.OnShowRewards -= Setup;
        }
        private void SubscribeToPathPicker(PathPicker pathPicker)
        {
            pathPicker.onClickSign += ShowDirection;
        }
        private void UnsubscribeToPathPicker(PathPicker pathPicker)
        {
            pathPicker.onClickSign -= ShowDirection;
        }

        protected override void CleanUp()
        {
            RewardManager.IfReady(UnsubscribeFromRewardManager);
            PathPicker.IfReady(UnsubscribeToPathPicker);
        }

        private void Setup(SerializableDictionary<Direction, SerializableDictionary<Reward, int>> rewards)
        {
            Setup(rewards.Keys.ToList());
        }

        public void Setup(List<Direction> avaliableDirections)
        {
            avaliableDirections.ForEach(a => Debug.Log(a));
            northArrow.gameObject.SetActive(avaliableDirections.Contains(Direction.North));
            southArrow.gameObject.SetActive(avaliableDirections.Contains(Direction.South));
            eastArrow.gameObject.SetActive(avaliableDirections.Contains(Direction.East));
            westArrow.gameObject.SetActive(avaliableDirections.Contains(Direction.West));
        }


        public void ShowCurrentDirection()
        {
            if (PathPicker.Instance.PickedDirection != null)
                ShowDirection((Direction)PathPicker.Instance.PickedDirection);
            else Setup(GlobalMapGenerator.Instance.CurrentMapPoint.connectedToPoints.Keys.ToList());
        }

        public void ShowDirection(Direction direction)
        {
            gameObject.SetActive(true);

            currentDirection = direction;

            directionText.text = direction.ToString();

            foreach (RewardItemUI item in rewardItemUIs)
            {
                Destroy(item.gameObject);
            }

            rewardItemUIs.Clear();

            if(!RewardManager.Instance.LevelRewards.TryGetValue(direction, out SerializableDictionary<Reward, int> rewards))
            {
                Debug.LogWarning($"Tried to selected an invalid direction: {direction}");
                return;
            }

            northArrow.sprite = normalArrowSprite;
            southArrow.sprite = normalArrowSprite;
            eastArrow.sprite = normalArrowSprite;
            westArrow.sprite = normalArrowSprite;

            foreach (KeyValuePair<Reward, int> reward in rewards)
            {
                RewardItemUI item = Instantiate(rewardItemUIPrefab, rewardItemsHolder);

                Sprite sprite = null;

                switch (reward.Key)
                {
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

            northArrow.sprite = normalArrowSprite;
            southArrow.sprite = normalArrowSprite;
            eastArrow.sprite = normalArrowSprite;
            westArrow.sprite = normalArrowSprite;

            switch (direction)
            {
                case Direction.South:
                    southArrow.sprite = hightlightedArrowSprite;
                    break;
                case Direction.North:
                    northArrow.sprite = hightlightedArrowSprite;
                    break;
                case Direction.East:
                    eastArrow.sprite = hightlightedArrowSprite;
                    break;
                case Direction.West:
                    westArrow.sprite = hightlightedArrowSprite;
                    break;
            }

            SFXManager.Instance.PlayOpenWindow();
        }


        public void ButtonSetup(int dir)
        {
            ShowDirection((Direction)dir);

            SFXManager.Instance.PlayButtonClick();
        }

        public void OnPickDirectionButton()
        {
            PathPicker.Instance.PickDirection(currentDirection);

            SFXManager.Instance.PlayButtonClick();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            SFXManager.Instance.PlayCloseWindow();
        }


    }

}
