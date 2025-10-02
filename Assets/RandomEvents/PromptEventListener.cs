using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DialogueSystem
{
    public class PromptEventListener : MonoBehaviour
    {
        [SerializeField]
        private Prompt prompt;
        [SerializeField]
        private int promptResponseIndex;
        private PromptResponse promptResponse
        {
            get 
            { 
                if(prompt != null && 
                    prompt.promptResponses != null && 
                    prompt.promptResponses.Count > 0 && 
                    promptResponseIndex.Between(0, prompt.promptResponses.Count - 1))
                {
                    return prompt.promptResponses[promptResponseIndex];
                }

                return null;
            } 
        }

        public UnityEvent Event;

        private void OnEnable()
        {
            promptResponse?.RegisterListener(this);
        }

        private void OnDisable()
        {
            promptResponse?.UnregisterListener(this);
        }

        public void OnEventRaised()
        {
            Event?.Invoke();
        }
    }
}


