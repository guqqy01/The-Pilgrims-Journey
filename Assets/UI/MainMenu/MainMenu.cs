using UnityEngine;
using UnityEngine.UI;
using Monsters;
using Helper.Wait;
using System.Collections.Generic;


namespace UI
{
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

        [SerializeField]
        private AudioClipAndSettings impactClip;
        [SerializeField]
        private float stopThemeTime = 0.5f;
        public bool Deactivated = false;
        [SerializeField]

        private List<Button> buttons;

        protected override void Awake()
        {
            base.Awake();
            Deactivated = false;
        }

        private void Start()
        {
            Setup();

            Deactivated = false;
        }

        private void OnEnable()
        {
            Setup();
            ThemeManager.WhenReady(PlayTheme);
        }

        private void PlayTheme(ThemeManager manager)
        {
            manager.PlayTheme(mainMenuTheme);
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

        public void HideHeroSelectionScreen()
        {
            HeroSelectionWindow.IfReady(manager => manager.gameObject.SetActive(false));
            buttons.ForEach((button) => button.interactable = true);
        }

        private void ShowHeroSelectionScreen()
        {
            HeroSelectionWindow.WhenReady(HeroSelectSetActiveAndUpdateUI);

            buttons.ForEach((button) => button.interactable = false);
        }

        private void HeroSelectSetActiveAndUpdateUI(HeroSelectionWindow manager)
        {
            manager.gameObject.SetActive(false);
            manager.UpdateUI();
        }

        private void StartNewJourney(bool value)
        {
            if (value)
            {
                Debug.Log(1);

                if (Deactivated)
                    return;

                Deactivated = true;

                ShowHeroSelectionScreen();
            }
        }

        public void PlayImpactAndStopTheme()
        {
            impactClip.Play();
            ThemeManager.IfReady((manager) => manager.StopTheme(stopThemeTime));
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
            PlayImpactAndStopTheme();

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
}

