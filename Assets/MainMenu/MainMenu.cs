using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Monsters;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { private set; get; }

    [SerializeField]
    private InputQuery inputQuery;

    [SerializeField]
    private GameObject unitCatalogue;
    [SerializeField]
    private GameObject equipmentCatalogue;
    [SerializeField]
    private GameObject upgradeCatalogue;
    [SerializeField]
    private GameObject beasaryCatalogue;
    [SerializeField]
    private GameObject relicCatalogue;

    [SerializeField]
    private Button continueJourneyButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BeastaryManager.Instance.Load();

        Setup();
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

    private void StartNewJourney(bool value)
    {
        if (value)
        {
            GameManager.Instance.StartNewJourney();
            Time.timeScale = 1.0f;
        }
    }

    public void ContinueJourneyButton()
    {
        GameManager.Instance.ContinueJourney();
        Time.timeScale = 1.0f;
    }

    public void UpgradeButton()
    {
        OpenPage(Pages.Upgrade);
    }

    public void BeastaryButton()
    {
        OpenPage(Pages.Beastary);
    }

    public void OpenOptions()
    {
        OpenPage(Pages.Options);
    }

    public void RelicButton()
    {
        OpenPage(Pages.Relic);
    }

    public void OpenEquipmentCatalogue()
    {
        OpenPage(Pages.Equipment);
    }

    public void OpenUnitCatalogue()
    {
        OpenPage(Pages.Units);
    }

    private void OpenPage(Pages page)
    {
        equipmentCatalogue.SetActive(page == Pages.Equipment);
        unitCatalogue.SetActive(page == Pages.Units);
        upgradeCatalogue.SetActive(page == Pages.Upgrade);
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
        Upgrade,
        Options,
        Beastary,
        Units,
        Equipment,
        Relic,
    }
}
