using UnityEngine;
using UnityEngine.UI;

namespace Relics
{
    public class RelicSpellUIItem : MonoBehaviour
    {
        private SpellSO spell;
        [SerializeField]
        private Image spellSpriteImage;
        [SerializeField]
        private Text spellTitleText;
        [SerializeField]
        private Text spellDescriptionText;
        [SerializeField]
        private Text spellAmountOfUsesText;

        public void Setup(RelicSpell relicSpell)
        {
            spell = relicSpell.spell;

            spellSpriteImage.sprite = spell.sprite;

            spellTitleText.text = spell.name.AddSpacesBeforeCapsRegex();
            spellDescriptionText.text = spell.description;
            spellAmountOfUsesText.text = $"Amount of uses per round: {relicSpell.amountOfUses.ToString()}";
        }
    }
}

