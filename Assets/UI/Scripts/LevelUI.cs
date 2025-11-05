using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;
using GameManagerCalls;
using TMPro;

public class LevelUI : MonoBehaviour, IOnAfterCreateLocalMap
{
    [SerializeField]
    private TextMeshProUGUI requiredUnitsText;

    [SerializeField]
    private Image progressBarFill;

    private void OnEnable()
    {
        LocalMapLevelCompletedManager.onUnitPassed += OnUnitPassed;
    }

    private void OnDisable()
    {
        LocalMapLevelCompletedManager.onUnitPassed -= OnUnitPassed;
    }

    public void LevelChange()
    {
        int requiredToPass = LocalMapLevelCompletedManager.Instance.RequiredNumberOfUnitsToPass;  

        requiredUnitsText.text =  "0/" + requiredToPass;
        progressBarFill.fillAmount = Mathf.Clamp01(0 / requiredToPass);
    }

    public void OnUnitPassed(int totalPassed, int requiredToPass)
    {
        requiredUnitsText.text = totalPassed + "/" + requiredToPass;
        progressBarFill.fillAmount = Mathf.Clamp01((float)totalPassed / requiredToPass);
    }

    public void AfterCreateLocalMap()
    {
        LevelChange();
    }
}
