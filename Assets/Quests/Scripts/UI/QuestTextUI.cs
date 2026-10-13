using Quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace TPJ.UI
{
    public class QuestTextUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI questTitle;
        [SerializeField]
        private TextMeshProUGUI questDescription;
        [SerializeField]
        private TextMeshProUGUI questProgress;
        [SerializeField]
        private Image checkbox;

        [SerializeField]
        private Sprite inProgressSprite;
        [SerializeField]
        private Sprite completeSprite;
        [SerializeField]
        private Sprite failedSprite;
        [SerializeField]
        private Color inProgressColor;
        [SerializeField]
        private Color completedColor;
        [SerializeField]
        private Color failedColor;

        private Quest _quest;

        private bool _showDescription;

        public void SetupQuest(Quest quest, bool showDescription)
        {
            _quest = quest;
            _showDescription = showDescription;

            UpdateUI(quest);
            quest.onChanged += UpdateUI;
        }

        private void UpdateUI(Quest quest)
        {
            questDescription.gameObject.SetActive(_showDescription);

            questTitle.text = quest.Title;
            questDescription.text = quest.Description;
            questProgress.text = quest.ProgressText;
            switch (quest.State)
            {
                case QuestState.Active:
                    questTitle.color = inProgressColor;
                    questDescription.color = inProgressColor;
                    checkbox.sprite = inProgressSprite;
                    questProgress.color = failedColor;
                    break;
                case QuestState.Completed:
                    questTitle.color = completedColor;
                    questDescription.color = completedColor;
                    questProgress.color = completedColor;
                    checkbox.sprite = completeSprite;
                    break;
                case QuestState.Failed:
                    questTitle.color = failedColor;
                    questTitle.fontStyle |= FontStyles.Strikethrough;
                    questDescription.color = failedColor;
                    questProgress.color = failedColor;
                    checkbox.sprite = failedSprite;
                    break;
            }
        }

        private void OnDestroy()
        {
            if (_quest != null)
            {
                _quest.onChanged -= UpdateUI;
            }
        }
    }
}
