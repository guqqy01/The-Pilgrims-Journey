using CodiceApp.EventTracking;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI 
{
    public class InputQuery : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI queryText;
        [SerializeField]
        private TextMeshProUGUI yesText;
        [SerializeField]
        private TextMeshProUGUI noText;

        private Action<bool> _action;

        [SerializeField]
        private GameObject focusElement;

        private GameObject previousSelectedObject;

        public void Setup(string queryString, Action<bool> action, string yesString = "Yes", string noString = "No")
        {
            queryText.text = queryString;
            yesText.text = yesString;
            noText.text = noString;
            this._action = action;

            previousSelectedObject = SelectionManager.Instance.GetCurrentlySelected();
            SelectionManager.Instance.FocusOnElement(focusElement);
        }

        public void ButtonPress(bool value)
        {
            _action(value);
            gameObject.SetActive(false);
            SelectionManager.Instance.FocusOnElement(previousSelectedObject);
        }
    }
}
