using CodiceApp.EventTracking;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI 
{
    public class SelectionManager : Singleton<SelectionManager>
    {
        private void Awake()
        {
            InitiateFromRootAtAwake();
        }

        public GameObject GetCurrentlySelected()
        {
            return EventSystem.current.currentSelectedGameObject;
        }

        public void FocusOnElement(GameObject focusedElement)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(focusedElement);
        }
    }
}
