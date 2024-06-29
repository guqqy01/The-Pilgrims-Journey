using Map;
using System;
using UnityEngine;
using System.Collections.Generic;
public class MapPointRequiredUIManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public List<GameObject> optionalUIs;

    public List<MapPointTypeAndRequiredUI> mapPointTypeAndRequiredUIs;

    public static MapPointRequiredUIManager Instance;

    private void Start()
    {
        GameManager.onCreateCurrentMapPoint += Setup;
    }

    public void Setup()
    {
        MapPointTypeAndRequiredUI currentNeeded = null;

        for (int i = 0; i < mapPointTypeAndRequiredUIs.Count; i++)
        {
            if (mapPointTypeAndRequiredUIs[i].mapPointType == GlobalMapGenerator.Instance.CurrentMapPoint.Type)
            {
                currentNeeded = mapPointTypeAndRequiredUIs[i];
                break;
            }
        }

        TopBarMovement.Instance.ControlMovementOfBar = currentNeeded.controlTopBarMovement;

        foreach(GameObject optionalUI in optionalUIs)
        {
            optionalUI.SetActive(currentNeeded.requiredObjects.Contains(optionalUI));
        }
    }

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
}


[Serializable]
public class MapPointTypeAndRequiredUI
{
    public GlobalMapPointType mapPointType;
    public List<GameObject> requiredObjects;
    public bool controlTopBarMovement = false;
}