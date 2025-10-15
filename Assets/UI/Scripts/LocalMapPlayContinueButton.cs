using GameManagerCalls;
using Map;
using System.Collections;
using System.Collections.Generic;
using Tiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LocalMapPlayContinueButton : MonoBehaviour, IOnCompletedLevel, IOnCreateLocalMap
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private Text text;
    [SerializeField]
    private GameObject secondArrow;

    private bool completedLevel = false;

    public void OnLevelCompleted() 
    {
        text.text = "Continue";
        secondArrow.SetActive(true);
        button.interactable = true;
        completedLevel = true;
    }

    public void OnCreateLocalMap()
    {
        text.text = "Play";
        secondArrow.SetActive(false);
        button.interactable = true;
        completedLevel = false;
    }

    public void OnButtonClick()
    {
        if (!completedLevel)
        {
            TilePlayerManager.Instance.Play();
            text.text = "Playing";
            button.interactable = false;
        }
        else
        {
            SceneLoader.Instance.LoadGlobalMapScene();
            //GameManager.Instance.LoadCurrentMapPoint();
            GlobalMapUI.FirstSeen = true;
        }
    }
}
