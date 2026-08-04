using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Spells.UI
{
    public class SpellSubDescriptionUI : SubDescriptionUI
    {
        [SerializeField]
        private Image spellSpriteImage;
        [SerializeField]
        private TextMeshProUGUI spellTitleText;
        [SerializeField]
        private TextMeshProUGUI spellDescriptionText;
        [SerializeField]
        private TextMeshProUGUI spellAmountOfUsesText;

        public void Setup(SpellSubDescriptionData spellSubDescriptionData)
        {
            spellSpriteImage.sprite = spellSubDescriptionData.Sprite;

            spellTitleText.text = spellSubDescriptionData.Name.AddSpacesBeforeCapsRegex();
            spellDescriptionText.text = spellSubDescriptionData.Description;
            spellAmountOfUsesText.text = $"Amount of uses per round: {spellSubDescriptionData.AmountOfUses.ToString()}";
        }

        public override void UpdateUI(ISubDescriptionData subDescriptionData)
        {
            if(subDescriptionData is not SpellSubDescriptionData spellSubDescriptionData)
            {
                Debug.LogWarning($"Sub Description Data: {subDescriptionData} not expected Type: {typeof(SpellSubDescriptionData)}");
                return;
            }

            Setup(spellSubDescriptionData);
        }
    }
}

