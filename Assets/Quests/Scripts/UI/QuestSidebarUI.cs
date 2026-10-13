using UnityEngine;
using System.Collections.Generic;
using Quests;
using TMPro;
using GameManagerCalls;
namespace TPJ.UI
{
    public class QuestSidebarUI : Singleton<QuestSidebarUI>, IOnPlayLocalLevel
    {
        [SerializeField]
        private QuestTextUI questTextUIPrefab;
        private readonly List<QuestTextUI> _questTextUIs = new List<QuestTextUI>();

        [SerializeField]
        private TextMeshProUGUI questTitlePrefab;

        private readonly List<TextMeshProUGUI> _questTitleUIs = new List<TextMeshProUGUI>();

        private void Start()
        {
            QuestManager.WhenReady(questManager => questManager.OnQuestsGenerated += QuestsGenerated);
        }

        protected override void CleanUp()
        {
            QuestManager.IfReady(questManager => questManager.OnQuestsGenerated -= QuestsGenerated);
        }

        private void QuestsGenerated()
        {
            UpdateUI(true);
        }

        private void UpdateUI(bool showDescriptions)
        {
            _questTextUIs.DestroyAndClearList();
            _questTitleUIs.DestroyAndClearList();

            AddTitle("Main Quests");

            foreach (Quest quest in QuestManager.Instance.MainQuests)
            {
                QuestTextUI questTextUI = Instantiate(questTextUIPrefab, transform);
                questTextUI.SetupQuest(quest, showDescriptions);
                _questTextUIs.Add(questTextUI);
            }

            AddTitle("Optional");

            foreach (Quest quest in QuestManager.Instance.OptionalQuests)
            {
                QuestTextUI questTextUI = Instantiate(questTextUIPrefab, transform);
                questTextUI.SetupQuest(quest, showDescriptions);
                _questTextUIs.Add(questTextUI);
            }
        }

        private void AddTitle(string title)
        {
            TextMeshProUGUI titleUI = Instantiate(questTitlePrefab, transform);
            titleUI.text = title;
            _questTitleUIs.Add(titleUI);
        }

        public void OnPlayLocalLevel()
        {
            UpdateUI(false);
        }
    }
}
