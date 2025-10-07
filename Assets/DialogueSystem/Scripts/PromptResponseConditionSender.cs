using System;
using UnityEngine;

namespace DialogueSystem
{
    [Serializable]
    public class PromptResponseConditionSender
    {
        public Prompt prompt;
        public int responseIndex;

        [SerializeField]
        private BoolFunctionReference boolFunctionReference;

        public void Register()
        {
            if (prompt != null && prompt.promptResponses != null && prompt.promptResponses.Count > responseIndex)
            {
                var promptResponse = prompt.promptResponses[responseIndex];

                if (!promptResponse.conditionCallback.conditions.Contains(boolFunctionReference.Invoke))
                    promptResponse.conditionCallback.conditions.Add(boolFunctionReference.Invoke);
            }
        }

        public void Unregister()
        {
            if (prompt != null && prompt.promptResponses != null && prompt.promptResponses.Count > responseIndex)
            {
                var promptResponse = prompt.promptResponses[responseIndex];
                promptResponse.conditionCallback.conditions.Remove(boolFunctionReference.Invoke);
            }
        }
    }
}
