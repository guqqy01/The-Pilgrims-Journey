using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Relics
{
    public class RelicSpellUIItem : SubDescriptionUI
    {
        private SpellSO spell;
        [SerializeField]
        private Image spellSpriteImage;
        [SerializeField]
        private TextMeshProUGUI spellTitleText;
        [SerializeField]
        private TextMeshProUGUI spellDescriptionText;
        [SerializeField]
        private TextMeshProUGUI spellAmountOfUsesText;

        public void Setup(RelicSpell relicSpell)
        {
            spell = relicSpell.spell;

            spellSpriteImage.sprite = spell.sprite;

            spellTitleText.text = spell.name.AddSpacesBeforeCapsRegex();
            spellDescriptionText.text = spell.description;
            spellAmountOfUsesText.text = $"Amount of uses per round: {relicSpell.amountOfUses.ToString()}";
        }

        public override void UpdateUI(params object[] obj)
        {
            if (obj[0] is not RelicSpell relicSpell)
            {
                Destroy(gameObject);
                return;
            }

            Setup(relicSpell);
        }
    }
}

