using UnityEngine;
using UnityEngine.Events;
namespace DialogueSystem
{
    public class DialogueEventListener : MonoBehaviour
    {
        public Dialogue dialogue;
        public UnityEvent StartEvents;
        public UnityEvent EndEvents;

        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            Unregister();
        }

        public void Register()
        {
            dialogue?.RegisterListener(this);
        }

        public void Unregister()
        {
            dialogue?.UnregisterListener(this);
        }   

        public void OnStartEventRaised()
        {
            StartEvents?.Invoke();
        }

        public void OnEndEventRaised()
        {
            EndEvents?.Invoke();
        }
    }
}


