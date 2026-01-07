using UnityEngine;
using Heros;
using TMPro;

public class HeroMainDescriptionSection : MonoBehaviour, IHeroDescriptionSection
{
    [SerializeField]
    private TextMeshProUGUI heroNameText;
    [SerializeField]
    private TextMeshProUGUI descriptionText;
    [SerializeField]
    private TextMeshProUGUI statsText;

    public void Bind(HeroSO heroSO)
    {
        heroNameText.text = heroSO.name;

        descriptionText.text = heroSO.Description;

        statsText.text = heroSO.GetHeroStatsString();
    }

    public bool HasContent(HeroSO heroSO)
    {
        return heroSO != null;
    }
}
