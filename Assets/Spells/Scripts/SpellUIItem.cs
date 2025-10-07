using UnityEngine;
using UnityEngine.UI;

public class SpellUIItem : MonoBehaviour
{
    public Spell spell;

    public Text useKeyText;
    public Text amountOfUsesText;
    public Image spellImage;

    public void UpdateUI(Spell spell)
    {
        this.spell = spell;
        amountOfUsesText.text = spell.numberOfUses.ToString();
        spellImage.sprite = spell.spellSO.sprite;

        if (spell.numberOfUses > 0)
            spellImage.color = spellImage.color.SetAlpha(1f);
        else
            spellImage.color = spellImage.color.SetAlpha(0.5f);
    }

    public void Deactivate()
    {
        spell = null;
        amountOfUsesText.text = "";
        spellImage.sprite = null;
        spellImage.color = spellImage.color.SetAlpha(0f);
    }
}