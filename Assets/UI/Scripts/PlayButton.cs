using GameManagerCalls;
using Map;
using Tiles;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour, IOnCompletedLevel, IOnCreateLocalMap
{
    [SerializeField]
    private Button button;

    private bool completedLevel = false;

    public void OnLevelCompleted()
    {
        completedLevel = true;
    }

    public void OnCreateLocalMap()
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
    }
}
