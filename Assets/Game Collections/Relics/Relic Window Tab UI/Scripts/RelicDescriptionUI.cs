using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Spells.UI;

namespace Relics.UI
{
    public class RelicDescriptionUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI relicDescriptionText;

        [SerializeField]
        private SpellSubDescriptionUI relicSpellUIItemPrefab;

        private readonly List<SpellSubDescriptionUI> relicSpellUIItems = new List<SpellSubDescriptionUI>();

        [SerializeField]
        private Transform descriptionHolder;

        [SerializeField]
        private LayoutContentSizerBaseParent layoutSizerParent;

        public void Setup(Relic relic)
        {
            RelicSO relicSO = relic.relicSO;

            relicSpellUIItems.DestroyAndClearList();

            relicDescriptionText.text = GetDescriptionString(relic);

            foreach (SpellAndAmountOfUses relicSpell in relicSO.spells)
            {
                SpellSubDescriptionUI relicSpellUIItem = Instantiate(relicSpellUIItemPrefab, descriptionHolder);
                relicSpellUIItem.Setup(new SpellSubDescriptionData(relicSpell.spell.name, relicSpell.spell.Sprite, relicSpell.spell.MainDescription, relicSpell.amountOfUses));
                relicSpellUIItems.Add(relicSpellUIItem);
            }

            StartCoroutine(WaitForSetupThenUpdateAllLayouts());
        }

        private string GetDescriptionString(Relic relic)
        {
            string returnString = relic.relicSO.description;

            foreach (RelicEffect relicEffect in relic.relicEffects)
            {
                if (returnString != "")
                    returnString += "\r\n";
                returnString += relicEffect.Description(relic);
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

