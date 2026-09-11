using System.Collections.Generic;
using UnityEngine;
using Heros;
using Helper.Wait;
using UnityEngine.UI;
public class HeroSelectionWindow : Singleton<HeroSelectionWindow>
{
    [HideInInspector]
    public HeroSO HoveredHeroSO;

    [SerializeField]
    private HeroSelectionButtonUI HeroSelectionButtonUIPrefab;

    [SerializeField]
    private Transform heroSelectionButtonsHolder;

    private readonly List<HeroSelectionButtonUI> heroSelectionButtonUIs = new List<HeroSelectionButtonUI>();

    [SerializeField]
    private List<HeroSO> heroSOs = new List<HeroSO>();

    [SerializeField]
    private Button selectHeroButton;

    [SerializeField]
    private HeroMainDescriptionSection heroMainDescriptionSection;

    [SerializeField]
    private HeroWeaponDesciptionSection heroWeaponDesciptionSection;

    [SerializeField]
    private RectTransform contentTransform;

    public override void InitiateFromRootAtAwake()
    {
        base.InitiateFromRootAtAwake();
        gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        heroSelectionButtonUIs.DestroyAndClearList();

        foreach (var heroSO in heroSOs)
        {
            var buttonUI = Instantiate(HeroSelectionButtonUIPrefab, heroSelectionButtonsHolder);
            buttonUI.UpdateUI(heroSO);
            heroSelectionButtonUIs.Add(buttonUI);
        }

        selectHeroButton.interactable = HoveredHeroSO != null;

        UpdateDescriptionUI();

        contentTransform.anchoredPosition = Vector2.zero;
    }

    public void HoverHero(HeroSO heroSO)
    {
        HoveredHeroSO = heroSO;
        selectHeroButton.interactable = HoveredHeroSO != null;
        UpdateDescriptionUI();
    }

    public void SelectHoveredHero()
    {
        SelectHero(HoveredHeroSO);
    }

    public void SelectHero(HeroSO heroSO)
    {
        if(heroSO == null)
            return;

        gameObject.SetActive(false);

        MainMenu.IfReady((mainMenu) => mainMenu.PlayImpactAndStopTheme());

        GameManager.Instance.StartNewJourney(heroSO);

        WaitForHelper.WaitForObjectThenPerformAction(() => TimeManager.Instance, () => TimeManager.Instance.OverrideUnpauseTime());
    }

    

    private void UpdateDescriptionUI()
    {
        UpdateHeroDescriptionUI();
        UpdateWeaponDescriptionUI();
    }

    private void UpdateHeroDescriptionUI()
    {
        bool hasContent = heroMainDescriptionSection.HasContent(HoveredHeroSO);

        heroMainDescriptionSection.gameObject.SetActive(hasContent);

        if (!hasContent)
            return;

        heroMainDescriptionSection.Bind(HoveredHeroSO);
    }

    private void UpdateWeaponDescriptionUI()
    {
        bool hasContent = heroWeaponDesciptionSection.HasContent(HoveredHeroSO);

        heroWeaponDesciptionSection.gameObject.SetActive(hasContent);

        if (!hasContent)
            return;

        heroWeaponDesciptionSection.Bind(HoveredHeroSO);
    }
}
