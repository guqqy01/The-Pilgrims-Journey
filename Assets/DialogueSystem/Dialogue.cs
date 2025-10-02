using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DialogueSystem
{
    [CreateAssetMenu(menuName = "DialogueSystem/Dialogue")]
    public class Dialogue : ScriptableObject
    {
        public Prompt initialPrompt;
        public NextPromptSetupSettings settings;

        public readonly List<DialogueEventListener> listeners = new();

        public Dialogue nextDialogue;

        public void RaiseStartEvents()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
                listeners[i].OnStartEventRaised();
        }
        public void RaiseEndEvents()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
                listeners[i].OnEndEventRaised();
        }

        public void RegisterListener(DialogueEventListener listener)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void UnregisterListener(DialogueEventListener listener)
        {
            listeners.Remove(listener);
        }
    }

}
