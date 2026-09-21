using UnityEngine;
using System.Collections.Generic;
using UI.CardCarousel;

namespace LevelingSystem.UI
{
    public class LevelingUpRewardUIManager : MonoBehaviour
    {
        [SerializeField]
        private LevelingUpRewardUIItem levelingUpRelicUIItemPrefab;

        [SerializeField]
        private Carousel carousel;

        private readonly List<LevelingUpRewardUIItem> uiItems = new();

        [SerializeField]
        private Transform uiItemsHolder;

        [SerializeField]
        private GameObject window;

        private const string blurRequestReference = "Leveling Up Reward UI manager";

        private void Awake()
        {
            LevelingUpRewardManager.Instance.onShowUI += ShowUI;
        }

        private void OnDestroy()
        {
            if (LevelingUpRewardManager.Instance != null)
                LevelingUpRewardManager.Instance.onShowUI -= ShowUI;
        }

        public void ShowUI(Dictionary<ILevelingUpReward, int> rewards)
        {
            uiItems.DestroyAndClearList();

            List<CarouselCard> cards = new List<CarouselCard>();

            foreach (var rewardPair in rewards)
            {
                LevelingUpRewardUIItem uiItem =
                    Instantiate(levelingUpRelicUIItemPrefab, uiItemsHolder);

                if(uiItem.TryGetComponent(out CarouselCard card))
                {
                    cards.Add(card);
                }

                uiItem.Setup(rewardPair.Key, rewardPair.Value);

                uiItems.Add(uiItem);
            }

            carousel.AddCards(cards);

            BlurBackgroundController.Instance.EnableBlur(blurRequestReference);
            window.SetActive(true);
        }

        public void Select()
        {
            if (carousel.CurrentSelectedCard == null)
                return;

            if(!carousel.CurrentSelectedCard.TryGetComponent(out LevelingUpRewardUIItem item))
                return;

            OnSelectReward(item.Reward);
        }

        private void OnSelectReward(ILevelingUpReward reward)
        {
            LevelingUpRewardManager.Instance.SelectReward(reward);

            window.SetActive(false);
            BlurBackgroundController.Instance.RemoveBlurRequest(blurRequestReference);
        }
    }
}
