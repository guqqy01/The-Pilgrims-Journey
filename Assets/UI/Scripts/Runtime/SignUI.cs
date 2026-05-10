using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalMap;
using System.Dynamic;
using System.Linq;
using TMPro;
using Helper.Wait;

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

        public override void InitiateFromRootAtAwake()
        {
            base.InitiateFromRootAtAwake();

            WaitForHelper.WaitForObjectThenPerformAction(() => PathPicker.Instance, SubscribeToOnClickSign);
        }

        private void Start()
        {
            RewardManager.Instance.OnShowRewards += Setup;
        }

        private void OnDestroy()
        {
            if(RewardManager.Instance != null)
            {
                RewardManager.Instance.OnShowRewards -= Setup;
            }
        }

        private void SubscribeToOnClickSign()
        {
            PathPicker.Instance.onClickSign -= ShowDirection;
            PathPicker.Instance.onClickSign += ShowDirection;
        }

        protected override void CleanUp()
        {
            if (PathPicker.Instance != null)
                PathPicker.Instance.onClickSign -= ShowDirection;
        }

        private void Setup(SerializableDictionary<Direction, SerializableDictionary<Reward, int>> rewards)
        {
            Setup(rewards.Keys.ToList());
        }

        public void Setup(List<Direction> avaliableDirections)
        {
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

            AudioManager.Instance.PlayOpenWindow();
        }


        public void ButtonSetup(int dir)
        {
            ShowDirection((Direction)dir);

            AudioManager.Instance.PlayButtonClick();
        }

        public void OnPickDirectionButton()
        {
            PathPicker.Instance.PickDirection(currentDirection);

            AudioManager.Instance.PlayButtonClick();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            AudioManager.Instance.PlayCloseWindow();
        }


    }

}
