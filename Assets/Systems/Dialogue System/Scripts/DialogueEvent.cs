using System;
using UnityEngine;
namespace DialogueSystem
{
    [Serializable]
    public class DialogueEvent
    {
        private delegate void OnRaised();
        private event OnRaised Raised;

        private bool debugEvents = false;

        public void Raise(string eventName)
        {
            if(debugEvents)
                Debug.Log($"Raise Event: {eventName}");
            Raised?.Invoke();
        }

        public void RegisterListener(DialogueEventListener listener)
        {
            Raised += listener.OnEventRaised;
        }

        public void UnregisterListener(DialogueEventListener listener)
        {
            Raised -= listener.OnEventRaised;
        }
    }
}


