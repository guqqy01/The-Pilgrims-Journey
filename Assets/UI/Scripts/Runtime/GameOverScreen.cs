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

    public override void InitiateFromRootAtAwake()
    {
        base.InitiateFromRootAtAwake();

        gameObject.SetActive(false);
    }

    public void OnGameEnd(string message)
    {
        gameObject.SetActive(true);
        reasonMessage.text = message;
    }

    public void OnSetupNewGame()
    {
        gameObject.SetActive(false);
    }

    public void SetupContinueGameAfterMapPointLoad()
    {
        gameObject.SetActive(false);
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
