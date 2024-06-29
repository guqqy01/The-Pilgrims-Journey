using Map;
using System.Collections;
using System.Collections.Generic;
using Tiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LocalMapPlayContinueButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private Text text;
    [SerializeField]
    private GameObject secondArrow;

    private bool completedLevel = false;

    private void OnEnable()
    {
        GameManager.onCompleteLevel += OnCompleteLevel;
        GameManager.onCreateLocalMap += OnCreateLocalMap;
    }

    private void OnDisable()
    {
        GameManager.onCompleteLevel -= OnCompleteLevel;
        GameManager.onCreateLocalMap -= OnCreateLocalMap;
    }

    private void OnCompleteLevel() 
    {
        text.text = "Continue";
        secondArrow.SetActive(true);
        button.interactable = true;
        completedLevel = true;
    }

    private void OnCreateLocalMap()
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
