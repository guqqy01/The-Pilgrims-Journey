using GameManagerCalls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour, ISingletonToBeInitiatedFromRoot
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
    }

    private void OnEnable()
    {
        GameManager.onSetup += Setup;
        GameManager.onGameEnd += GameOver;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        GameManager.onSetup -= Setup;
        GameManager.onGameEnd -= GameOver;
        gameObject.SetActive(false);
    }

    private void GameOver()
    {
        gameObject.SetActive(true);
    }

    private void Setup()
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
