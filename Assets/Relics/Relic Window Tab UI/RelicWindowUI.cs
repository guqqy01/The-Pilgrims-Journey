using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Relics
{
    public class RelicWindowUI : MonoBehaviour
    {
        private RelicManager relicManager;
        [SerializeField]
        private RelicItemUI relicItemUIPrefab;

        private List<RelicItemUI> relicItemsUIs = new List<RelicItemUI>();

        [SerializeField]
        private Transform relicItemUISParent;

        [SerializeField]
        private Text relicTitleText;
        [SerializeField]
        private Text relicDescriptionText;

        private void Start()
        {
            relicManager = RelicManager.Instance;
            relicManager.updateUI += UpdateUI;
            UpdateUI();

            if (relicItemsUIs.Count > 0)
            {
                relicItemsUIs[0].Select();
            }
        }

        public void UpdateUI()
        {
            SetupRelicItemUIs();
        }

        private void SetupRelicItemUIs()
        {
            relicItemsUIs.DestroyAndClearList();

            foreach (Relic relic in relicManager.Relics)
            {
                RelicItemUI itemUI = Instantiate(relicItemUIPrefab, relicItemUISParent);
                itemUI.onSelect += OnRelicSelected;
                itemUI.Setup(relic);
                relicItemsUIs.Add(itemUI);
            }
        }

        private void OnRelicSelected(Relic relicSelected)
        {
            relicTitleText.text = relicSelected.relicSO.name;
            relicDescriptionText.text = relicSelected.relicSO.description;

            foreach(RelicItemUI itemUI in relicItemsUIs)
            {
                if(itemUI.Relic != relicSelected)
                    itemUI.Unselect();
            }
        }
    }

}
