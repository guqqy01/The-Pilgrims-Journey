using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
namespace DialogueSystem
{
    [Serializable]
    public class PromptResponse
    {
        [TextArea(1, 5)]
        public string response;

        internal readonly List<PromptEventListener> listeners = new();
        public int NumberOfListeners => listeners.Count;


        public Prompt nextPrompt;
        public NextPromptSetupSettings nextPromptSetupSettings = NextPromptSetupSettings.GoToStart;
        public bool GoneDownPromptPath { get; set; }

        public void Raise()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
                listeners[i].OnEventRaised();
        }

        public void RegisterListener(PromptEventListener listener)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void UnregisterListener(PromptEventListener listener)
        {
            listeners.Remove(listener);
        }
    }
}
