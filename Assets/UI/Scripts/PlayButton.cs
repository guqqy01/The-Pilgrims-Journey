using Map;
using Tiles;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    private bool completedLevel = false;

    private void Awake()
    {
        GameManager.onCompleteLevel += OnCompleteLevel;
        GameManager.onCreateLocalMap += OnCreateLocalMap;
    }

    private void OnCompleteLevel()
    {
        completedLevel = true;
    }

    private void OnCreateLocalMap()
    {
        button.interactable = true;
        completedLevel = false;
    }

    public void OnButtonClick()
    {
        if (!completedLevel)
        {
            TilePlayerManager.Instance.Play();
            button.interactable = false;
        }
        /*else
        {
            SceneLoader.Instance.LoadGlobalMapScene();
            //GameManager.Instance.LoadCurrentMapPoint();
            GlobalMapUI.FirstSeen = true;
        }*/
    }
}
