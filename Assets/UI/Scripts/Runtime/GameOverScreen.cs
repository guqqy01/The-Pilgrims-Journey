using GameManagerCalls;
using Heros;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : Singleton<GameOverScreen>, IOnSetupNewGame, IOnGameEnd
{
    public override void InitiateFromRootAtAwake()
    {
        base.InitiateFromRootAtAwake();

        gameObject.SetActive(false);
    }

    public void OnGameEnd()
    {
        gameObject.SetActive(true);
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
