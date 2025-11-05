using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
namespace Relics
{
    public class RelicDescriptionUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI relicDescriptionText;

        [SerializeField]
        private RelicSpellUIItem relicSpellUIItemPrefab;

        private readonly List<RelicSpellUIItem> relicSpellUIItems = new List<RelicSpellUIItem>();

        [SerializeField]
        private Transform descriptionHolder;

        [SerializeField]
        private LayoutContentSizerBaseParent layoutSizerParent;

        public void Setup(Relic relic)
        {
            RelicSO relicSO = relic.relicSO;

            relicSpellUIItems.DestroyAndClearList();

            relicDescriptionText.text = GetDescriptionString(relic);

            foreach (RelicSpell relicSpell in relicSO.spells)
            {
                RelicSpellUIItem relicSpellUIItem = Instantiate(relicSpellUIItemPrefab, descriptionHolder);
                relicSpellUIItem.Setup(relicSpell);
                relicSpellUIItems.Add(relicSpellUIItem);
            }

            StartCoroutine("WaitForSetupThenUpdateAllLayouts");
        }

        private string GetDescriptionString(Relic relic)
        {
            RelicSO relicSO = relic.relicSO;

            string returnString = relic.relicSO.description;

            foreach (RelicEffect relicEffect in relic.relicEffects)
            {
                if (returnString != "")
                    returnString += "\r\n";
                returnString += relicEffect.Description;
            }

            return returnString;
        }

        private IEnumerator WaitForSetupThenUpdateAllLayouts()
        {
            yield return null;

            layoutSizerParent.UpdateAllLayoutsDeepestFirst();
        }
    }
}

