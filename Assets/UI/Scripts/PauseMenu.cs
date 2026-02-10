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
        BlurBackgroundController.Instance.EnableBlur();
    }

    public void ContinueButton()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1.0f;
        BlurBackgroundController.Instance.DisableBlur();
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
            BlurBackgroundController.Instance.DisableBlur();
        }
    }

    public void ExitMainMenuButton()
    {
        SceneLoader.Instance.LoadMainMenuScene();
        gameObject.SetActive(false);
        BlurBackgroundController.Instance.DisableBlur();

        if (!GameManager.Instance.LocalMapPlaying)
        {
            GameManager.Instance.Save();
        }
    }

    public void ExitDesktopButton()
    {
        if (!GameManager.Instance.LocalMapPlaying)
        {
            GameManager.Instance.Save();
        }
        Application.Quit();
    }
}
