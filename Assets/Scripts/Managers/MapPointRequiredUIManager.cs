using GlobalMap;
using System;
using UnityEngine;
using System.Collections.Generic;
using GameManagerCalls;
public class MapPointRequiredUIManager : Singleton<MapPointRequiredUIManager>, IOnCreateCurrentMapPoint
{
    public List<GameObject> optionalUIs;

    public List<MapPointTypeAndRequiredUI> mapPointTypeAndRequiredUIs;

    public void OnCreateCurrentMapPoint()
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
}


[Serializable]
public class MapPointTypeAndRequiredUI
{
    public GlobalMapPointType mapPointType;
    public List<GameObject> requiredObjects;
    public bool controlTopBarMovement = false;
}