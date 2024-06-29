using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;

public class LevelUI : MonoBehaviour
{
    [SerializeField]
    private Text requiredUnitsText;

    private void OnEnable()
    {
        GameManager.onAfterCreateLocalMap += LevelChange;
        GameManager.onUnitPassed += OnUnitPassed;
    }
    private void OnDisable()
    {
        GameManager.onAfterCreateLocalMap -= LevelChange;
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
}
