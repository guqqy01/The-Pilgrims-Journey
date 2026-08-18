using GameManagerCalls;
using Heros;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameOverScreen : Singleton<GameOverScreen>, IOnSetupNewGame, IOnGameEnd
{
    [SerializeField]
    private TextMeshProUGUI reasonMessage;

    private const string blurRef = "GameOver";

    public override void InitiateFromRootAtAwake()
    {
        base.InitiateFromRootAtAwake();

        Deactivate();
    }

    public void OnGameEnd(string message)
    {
        Activate();
        reasonMessage.text = message;
    }

    public void OnSetupNewGame()
    {
        Deactivate();
    }

    public void SetupContinueGameAfterMapPointLoad()
    {
        Deactivate();
    }

    private void Activate()
    {
        gameObject.SetActive(true);
        BlurBackgroundController.WhenReady(controller => controller.EnableBlur(blurRef));
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
        BlurBackgroundController.WhenReady(controller => controller.RemoveBlurRequest(blurRef));
    }

    public void TryAgainButton()
    {
        GameManager.Instance.StartNewJourney(HeroManager.Instance.HeroSO);
    }

    public void ExitMainMenuButton()
    {
        Action OnLoad = MainMenu.Instance.Setup;
        OnLoad  += () => MainMenu.Instance.Deactivated = false;
        SceneLoader.Instance.LoadSceneFromType(SceneLoadType.MainMenu ,OnLoad);
    }

    public void ExitDesktopButton()
    {
        Application.Quit();
    }

    
}
