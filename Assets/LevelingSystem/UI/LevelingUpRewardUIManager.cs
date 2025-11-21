using UnityEngine;
using System.Collections.Generic;
using Relics;
using Relics.UI;

namespace LevelingSystem.UI
{
    public class LevelingUpRewardUIManager : MonoBehaviour
    {
        [SerializeField]
        private LevelingUpRelicUIItem levelingUpRelicUIItemPrefab;

        private List<LevelingUpRelicUIItem> uIItems = new List<LevelingUpRelicUIItem>();

        [SerializeField]
        private Transform uIItemsHolder;

        [SerializeField]
        private GameObject window;

        private void Awake()
        {
            LevelingUpRewardManager.Instance.onShowUI += ShowUI;
        }

        private void OnDestroy()
        {
            if(LevelingUpRewardManager.Instance != null)
                LevelingUpRewardManager.Instance.onShowUI -= ShowUI;
        }

        public void ShowUI(Dictionary<RelicSO, int> relicSOs)
        {
            uIItems.DestroyAndClearList();

            foreach (var relic in relicSOs)
            {
                LevelingUpRelicUIItem uiItem = Instantiate(levelingUpRelicUIItemPrefab, uIItemsHolder);

                uiItem.Setup(relic.Key, relic.Value);

                uiItem.onButtonClick += OnButtonClick;

                uIItems.Add(uiItem);
            }

            window.SetActive(true);
        }

        private void OnButtonClick(RelicSO relic)
        {
            LevelingUpRewardManager.Instance.SelectRelic(relic);

            window.SetActive(false);
        }
    }
}

