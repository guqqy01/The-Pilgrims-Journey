using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Spells.UI
{
    public class SpellBottomBarUI : MonoBehaviour
    {
        private SpellManager manager;

        public List<SpellKeyPressBottomBarUIItem> spellUIItems = new List<SpellKeyPressBottomBarUIItem>();

        public int spellPageIndex = 0;
        public int TotalSpellPages => Mathf.Max(Mathf.CeilToInt(manager.Spells.Count / 4f), 1);

        public TextMeshProUGUI pagesText;

        private void Start()
        {
            manager = SpellManager.Instance;
            manager.updateUI += UpdateUI;

            UpdateUI();
        }

        private void Update()
        {
            if (InputManager.Spell_Selection.SelectSpell1.WasPressedThisFrame())
            {
                Spell1Button();
            }

            if (InputManager.Spell_Selection.SelectSpell2.WasPressedThisFrame())
            {
                Spell2Button();
            }

            if (InputManager.Spell_Selection.SelectSpell3.WasPressedThisFrame())
            {
                Spell3Button();
            }

            if (InputManager.Spell_Selection.SelectSpell4.WasPressedThisFrame())
            {
                Spell4Button();
            }

            if (InputManager.Spell_Selection.CancelSpell.WasPressedThisFrame())
            {
                manager.ActivateSpell(null);
            }
        }

        public void UpdateUI()
        {
            spellPageIndex = Mathf.Clamp(spellPageIndex, 0, TotalSpellPages - 1);

            pagesText.text = $"{spellPageIndex + 1}/{TotalSpellPages}";

            for (int i = 0; i < spellUIItems.Count; i++)
            {
                if (manager.Spells.Count >= spellPageIndex * 4 + i + 1)
                    spellUIItems[i].UpdateUI(manager.Spells[spellPageIndex * 4 + i]);
                else
                    spellUIItems[i].Deactivate();
            }
        }

        public void UpButtonPress()
        {
            spellPageIndex = Mathf.Min(spellPageIndex + 1, TotalSpellPages - 1);
            UpdateUI();
        }

        public void DownButtonPress()
        {
            spellPageIndex = Mathf.Max(spellPageIndex - 1, 0);
            UpdateUI();
        }

        public void Spell1Button()
        {
            if (spellUIItems[0].spell != null && spellUIItems[0].spell.numberOfUses > 0)
            {
                manager.ActivateSpell(spellUIItems[0].spell);
            }
        }

        public void Spell2Button()
        {
            if (spellUIItems[1].spell != null && spellUIItems[1].spell.numberOfUses > 0)
                manager.ActivateSpell(spellUIItems[1].spell);
        }

        public void Spell3Button()
        {
            if (spellUIItems[2].spell != null && spellUIItems[2].spell.numberOfUses > 0)
                manager.ActivateSpell(spellUIItems[2].spell);
        }

        public void Spell4Button()
        {
            if (spellUIItems[3].spell != null && spellUIItems[3].spell.numberOfUses > 0)
                manager.ActivateSpell(spellUIItems[3].spell);
        }
    }

}
