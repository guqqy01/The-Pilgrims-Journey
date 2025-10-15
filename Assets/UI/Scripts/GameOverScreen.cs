using GameManagerCalls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour, ISingletonToBeInitiatedFromRoot, IOnSetup, IOnGameEnd
{
    public static GameOverScreen Instance;

    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        gameObject.SetActive(false);
    }

    public void OnGameEnd()
    {
        gameObject.SetActive(true);
    }

    public void Setup()
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
