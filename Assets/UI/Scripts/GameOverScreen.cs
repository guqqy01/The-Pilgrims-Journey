using GameManagerCalls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : Singleton<GameOverScreen>, IOnSetupNewGame, IOnGameEnd
{
    public override void InitiateSingleton()
    {
        base.InitiateSingleton();

        gameObject.SetActive(false);
    }

    public void OnGameEnd()
    {
        gameObject.SetActive(true);
    }

    public void SetupNewGame()
    {
        gameObject.SetActive(false);
    }

    public void SetupContinueGameAfterMapPointLoad()
    {
        gameObject.SetActive(false);
    }

    public void TryAgainButton()
    {
        GameManager.Instance.StartNewJourney();
    }

    public void ExitMainMenuButton()
    {
        SceneLoader.Instance.LoadMainMenuScene(MainMenu.Instance.Setup);
    }

    public void ExitDesktopButton()
    {
        Application.Quit();
    }

    
}
