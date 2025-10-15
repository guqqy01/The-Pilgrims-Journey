using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;
using GameManagerCalls;

public class LevelUI : MonoBehaviour, IOnAfterCreateLocalMap
{
    [SerializeField]
    private Text requiredUnitsText;

    private void OnEnable()
    {
        GameManager.onUnitPassed += OnUnitPassed;
    }

    private void OnDisable()
    {
        GameManager.onUnitPassed -= OnUnitPassed;
    }

    public void LevelChange()
    {
        requiredUnitsText.text =  "0/" + GameManager.Instance.RequiredNumberOfUnitsToPass;
    }

    public void OnUnitPassed(int totalPassed, int requiredToPass)
    {
        requiredUnitsText.text = totalPassed + "/" + requiredToPass;
    }

    public void AfterCreateLocalMap()
    {
        LevelChange();
    }
}
