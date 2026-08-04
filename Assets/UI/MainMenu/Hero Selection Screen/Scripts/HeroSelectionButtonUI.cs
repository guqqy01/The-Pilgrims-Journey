using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Heros;
public class HeroSelectionButtonUI : MonoBehaviour
{
    private HeroSO heroSO;

    [SerializeField]
    private Image heroIconImage;
    [SerializeField]
    private Image attackIconImage;
    [SerializeField]
    private TextMeshProUGUI heroNameText;

    [SerializeField]
    private float heroImageSize = 60f;
    [SerializeField]
    private float attackImageSize = 40f;

    public void UpdateUI(HeroSO heroSO)
    {
        this.heroSO = heroSO;

        heroIconImage.sprite = heroSO.icon;
        heroIconImage.NativeRatioScaleToSize(heroImageSize);

        attackIconImage.sprite = heroSO.startingAttack.icon;
        attackIconImage.NativeRatioScaleToSize(attackImageSize);

        heroNameText.text = heroSO.name;
    }

    public void OnButtonClick()
    {
        HeroSelectionWindow.Instance.HoverHero(heroSO);
    }
}
