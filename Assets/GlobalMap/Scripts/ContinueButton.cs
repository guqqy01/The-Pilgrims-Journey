using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ContinueButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        GameManager.Instance.LoadCurrentMapPoint();
    }
}
