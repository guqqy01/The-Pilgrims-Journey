using System;
using UnityEngine;
using UnityEngine.Events;

namespace DialogueSystem
{
    [Serializable]
    public class PromptResponse
    {
        [TextArea(1, 5)]
        public string response;
        public UnityEvent responseAction;
        public Prompt nextPrompt;
        public NextPromptSetupSettings nextPromptSetupSettings = NextPromptSetupSettings.GoToStart;
        public bool GoneDownPromptPath { get; set; }
    }
}
