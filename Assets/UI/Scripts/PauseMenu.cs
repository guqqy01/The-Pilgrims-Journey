using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private InputQuery inputQuery;

    public void OpenPauseMenuButton()
    {
        Time.timeScale = 0f;
    }

    public void ContinueButton()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1.0f;
    }

    public void OptionsButton()
    {

    }

    public void AbandonRunButton()
    {
        inputQuery.gameObject.SetActive(true);
        inputQuery.Setup("Are you sure you want to abandon the current Journey?", AbandonRun);
    }

    public void AbandonRun(bool value)
    {
        if (value)
        {
            SaveManager.IfFileExistsDelete("GameManagerSave"); 
            SceneLoader.Instance.LoadMainMenuScene();
            gameObject.SetActive(false);
        }
    }

    public void ExitMainMenuButton()
    {
        GameManager.Instance.Save();
        SceneLoader.Instance.LoadMainMenuScene();
        gameObject.SetActive(false);
    }

    public void ExitDesktopButton()
    {
        GameManager.Instance.Save();
        Application.Quit();
    }
}
