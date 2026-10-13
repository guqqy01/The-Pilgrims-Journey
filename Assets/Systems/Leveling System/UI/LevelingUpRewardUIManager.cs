using UnityEngine;
using System.Collections.Generic;

namespace LevelingSystem.UI
{
    public class LevelingUpRewardUIManager : Singleton<LevelingUpRewardUIManager>
    {
        [SerializeField]
        private LevelingUpRewardUIItem levelingUpRelicUIItemPrefab;

        private readonly List<LevelingUpRewardUIItem> uiItems = new();

        [SerializeField]
        private Transform uiItemsHolder;

        [SerializeField]
        private GameObject window;

        private const string blurRequestReference = "Leveling Up Reward UI manager";

        protected override void Awake()
        {
            base.Awake();
            LevelingUpRewardManager.WhenReady(m => m.onShowUI += ShowUI);
        }

        protected override void CleanUp()
        {
            LevelingUpRewardManager.IfReady(m => m.onShowUI -= ShowUI);
        }

        public void ShowUI(Dictionary<ILevelingUpReward, int> rewards)
        {
            uiItems.DestroyAndClearList();

            foreach (var rewardPair in rewards)
            {
                LevelingUpRewardUIItem uiItem =
                    Instantiate(levelingUpRelicUIItemPrefab, uiItemsHolder);

                uiItem.Setup(rewardPair.Key, rewardPair.Value);

                uiItems.Add(uiItem);
            }

            BlurBackgroundController.Instance.EnableBlur(blurRequestReference);
            window.SetActive(true);
        }

        public void SelectReward(ILevelingUpReward reward)
        {
            LevelingUpRewardManager.Instance.SelectReward(reward);

            window.SetActive(false);
            BlurBackgroundController.Instance.RemoveBlurRequest(blurRequestReference);
        }
    }
}
