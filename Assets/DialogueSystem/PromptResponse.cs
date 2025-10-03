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

        public readonly DialogueEvent responseEvent = new DialogueEvent();
        public Prompt nextPrompt;
        public NextPromptSetupSettings nextPromptSetupSettings = NextPromptSetupSettings.GoToStart;
        public bool GoneDownPromptPath { get; set; }
    }
}
