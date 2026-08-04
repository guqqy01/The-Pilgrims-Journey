using UnityEngine;
using Heros;
using UnityEngine.UI;
using TMPro;

public class HeroWeaponDesciptionSection : MonoBehaviour, IHeroDescriptionSection
{
    [SerializeField]
    private TextMeshProUGUI weaponNameText;
    [SerializeField]
    private Image weaponIconImage;


    [SerializeField]
    private TextMeshProUGUI weaponDescriptionText;
    [SerializeField]
    private TextMeshProUGUI weaponStatsText;

    

    public void Bind(HeroSO heroSO)
    {
        weaponNameText.text = heroSO.startingAttack.name;
        weaponIconImage.sprite = heroSO.startingAttack.icon;
        weaponDescriptionText.text = heroSO.startingAttack.description;
        weaponStatsText.text = heroSO.startingAttack.GetStatsDescription(1);
    }

    public bool HasContent(HeroSO heroSO)
    {
        if(heroSO == null)
            return false;

        return heroSO.startingAttack != null;
    }
}
