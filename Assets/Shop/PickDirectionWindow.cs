using GlobalMap;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using LocalMap;

public class PickDirectionWindow : MonoBehaviour
{
    public GameObject northButtonGO;
    public GameObject eastButtonGO;
    public GameObject southButtonGO;

    public void Setup()
    {
        GlobalMapGenerator gMGInstance = GlobalMapGenerator.Instance;
        Dictionary<Direction, GlobalMapPoint> directionAndToPoint = gMGInstance.CurrentMapPoint.connectedToPoints;

        List<Direction> directions = directionAndToPoint.Keys.Where(x => !gMGInstance.takenPath.Contains(directionAndToPoint[x])).ToList();

        northButtonGO.SetActive(directions.Contains(Direction.North));
        eastButtonGO.SetActive(directions.Contains(Direction.East));
        southButtonGO.SetActive(directions.Contains(Direction.South));
    }

    public void NorthButtonClick()
    {
        ChooseDirection(Direction.North);
    }

    public void EastButtonClick()
    {
        ChooseDirection(Direction.East);
    }

    public void SouthButtonClick()
    {
        ChooseDirection(Direction.South);
    }

    private void ChooseDirection(Direction direction)
    {
        gameObject.SetActive(false);

        if (!GlobalMapGenerator.Instance.TryPickDirection(direction))
        {
            Debug.LogWarning($"Direction: {direction}, is not able to be picked");
        }
        GameManager.Instance.CompleteLevel();
        GameManager.Instance.LoadCurrentMapPoint();


    }
}
