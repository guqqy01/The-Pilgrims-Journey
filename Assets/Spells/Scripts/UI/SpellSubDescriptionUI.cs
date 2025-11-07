using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Spells.UI
{
    public class SpellSubDescriptionUI : SubDescriptionUI
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

        public void Setup(SpellSO spell, int amountOfUses)
        {
            this.spell = spell;

            spellSpriteImage.sprite = spell.sprite;

            spellTitleText.text = spell.name.AddSpacesBeforeCapsRegex();
            spellDescriptionText.text = spell.description;
            spellAmountOfUsesText.text = $"Amount of uses per round: {amountOfUses.ToString()}";
        }

        public override void UpdateUI(params object[] obj)
        {
            if (obj[0] is not SpellSO spell)
            {
                Destroy(gameObject);
                return;
            }

            if (obj[1] is not int amountOfUses)
            {
                Destroy(gameObject);
                return;
            }

            Setup(spell, amountOfUses);
        }
    }
}

