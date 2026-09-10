using UnityEngine;
using UnityEngine.UI;
using Monsters;
using Helper.Wait;

public class MainMenu : Singleton<MainMenu>
{
    [SerializeField]
    private InputQuery inputQuery;

    [SerializeField]
    private GameObject unitCatalogue;
    [SerializeField]
    private GameObject equipmentCatalogue;
    [SerializeField]
    private GameObject beasaryCatalogue;
    [SerializeField]
    private GameObject relicCatalogue;

    [SerializeField]
    private Button continueJourneyButton;

    [SerializeField]
    private Theme mainMenuTheme;

    public bool Deactivated = false;

    private void Awake()
    {
        Deactivated = false;
    }

    private void Start()
    {
        BeastaryManager.Instance.Load();

        Setup();

        Deactivated = false;
    }

    private void OnEnable()
    {
        Setup();
        ThemeManager.WhenReady((manager) => manager.PlayTheme(mainMenuTheme));
    }

    public void Setup()
    {
        continueJourneyButton.interactable = SaveManager.FileExists("GameManagerSave");

        OpenPage(Pages.None);
    }

    public void StartNewJourneyButton()
    {
        if (SaveManager.FileExists("GameManagerSave"))
        {
            inputQuery.gameObject.SetActive(true);
            inputQuery.Setup("Are you sure you want to start a new journey? this will delete the current journey.", StartNewJourney);
        }
        else
        {
            StartNewJourney(true);
        }
    }

    private void ShowHeroSelectionScreen()
    {
        HeroSelectionWindow.Instance.gameObject.SetActive(true);
        HeroSelectionWindow.Instance.UpdateUI();
    }

    private void StartNewJourney(bool value)
    {
        if (value)
        {
            if (Deactivated)
                return;

            Deactivated = true;

            ShowHeroSelectionScreen();
        }
    }

    public void ContinueJourneyButton()
    {
        if (Deactivated)
            return;

        Deactivated = true;

        if (!SaveManager.FileExists("GameManagerSave"))
        {
            Setup();
            return;
        }

        GameManager.Instance.ContinueJourney();
        WaitForHelper.WaitForObjectThenPerformAction(() => TimeManager.Instance, () => TimeManager.Instance.OverrideUnpauseTime());
    }

    public void BeastaryButton()
    {
        if (Deactivated)
            return;

        OpenPage(Pages.Beastary);
    }

    public void OpenOptions()
    {
        if (Deactivated)
            return;

        OpenPage(Pages.Options);
    }

    public void RelicButton()
    {
        if (Deactivated)
            return;

        OpenPage(Pages.Relic);
    }

    public void OpenEquipmentCatalogue()
    {
        if (Deactivated)
            return;

        OpenPage(Pages.Equipment);
    }

    public void OpenUnitCatalogue()
    {
        if (Deactivated)
            return;

        OpenPage(Pages.Units);
    }

    private void OpenPage(Pages page)
    {
        equipmentCatalogue.SetActive(page == Pages.Equipment);
        unitCatalogue.SetActive(page == Pages.Units);
        beasaryCatalogue.SetActive(page == Pages.Beastary);
        relicCatalogue.SetActive(page == Pages.Relic);
    }

    public void ExitDesktopButton()
    {
        Application.Quit();
    }

    private enum Pages
    {
        None,
        Options,
        Beastary,
        Units,
        Equipment,
        Relic,
    }
}
