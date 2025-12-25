using UnityEngine;
using System.Collections.Generic;
using LevelingSystem;
using Relics.UI;

namespace LevelingSystem.UI
{
    public class LevelingUpRewardUIManager : MonoBehaviour
    {
        [SerializeField]
        private LevelingUpRewardUIItem levelingUpRelicUIItemPrefab;

        private readonly List<LevelingUpRewardUIItem> uiItems = new();

        [SerializeField]
        private Transform uiItemsHolder;

        [SerializeField]
        private GameObject window;

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

            foreach (var rewardPair in rewards)
            {
                LevelingUpRewardUIItem uiItem =
                    Instantiate(levelingUpRelicUIItemPrefab, uiItemsHolder);

                uiItem.Setup(rewardPair.Key, rewardPair.Value);
                uiItem.onButtonClick += OnButtonClick;

                uiItems.Add(uiItem);
            }

            BlurBackgroundController.Instance.EnableBlur();
            window.SetActive(true);
        }

        private void OnButtonClick(ILevelingUpReward reward)
        {
            LevelingUpRewardManager.Instance.SelectReward(reward);

            window.SetActive(false);
            BlurBackgroundController.Instance.DisableBlur();
        }
    }
}
