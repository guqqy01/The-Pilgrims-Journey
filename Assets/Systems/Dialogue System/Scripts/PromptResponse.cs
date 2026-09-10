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

        public readonly PromptResponseConditionCallback ConditionCallback = new PromptResponseConditionCallback();
    }

    public class PromptResponseConditionCallback
    {
        public readonly List<Func<bool>> conditions = new List<Func<bool>>();

        public bool CanShow()
        {
            foreach (var condition in conditions)
            {
                if (!condition())
                    return false;
            }
            return true;
        }
    }

}
