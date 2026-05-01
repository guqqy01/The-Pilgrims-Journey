using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private InputQuery inputQuery;

    private const string pauseRequestReference = "Pause Menu";

    private const string blurRequestReference = "Pause Menu";

    public void OpenPauseMenuButton()
    {
        TimeManager.Instance.PauseTime(pauseRequestReference);
        BlurBackgroundController.Instance.EnableBlur(blurRequestReference);
    }

    public void ContinueButton()
    {
        gameObject.SetActive(false);
        TimeManager.Instance.RemovePauseRequest(pauseRequestReference);
        BlurBackgroundController.Instance.RemoveBlurRequest(blurRequestReference);
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
            SceneLoader.Instance.LoadSceneFromType(SceneLoadType.MainMenu, () => MainMenu.Instance.Deactivated = false);
            gameObject.SetActive(false);
            BlurBackgroundController.Instance.RemoveBlurRequest(blurRequestReference);
        }
    }

    public void ExitMainMenuButton()
    {
        SceneLoader.Instance.LoadSceneFromType(SceneLoadType.MainMenu, () => MainMenu.Instance.Deactivated = false);
        gameObject.SetActive(false);
        BlurBackgroundController.Instance.RemoveBlurRequest(blurRequestReference);

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
