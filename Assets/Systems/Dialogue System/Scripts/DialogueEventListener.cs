using System;
using UnityEngine;
using UnityEngine.Events;

namespace DialogueSystem
{
    [Serializable]
    public class DialogueEventListener
    {
        public DialogueEventType eventType;

        public Dialogue dialogue;
        public Prompt prompt;
        public int promptResponseIndex;
        public int promptParagraphIndex;

        public UnityEvent Event = new UnityEvent();

        private DialogueEvent DialogueEvent
        {
            get
            {
                switch (eventType) 
                {
                    case DialogueEventType.StartDialogue:
                        return dialogue.startDialogueEvent;
                    case DialogueEventType.EndDialogue:
                        return dialogue.endDialogueEvent;
                    case DialogueEventType.PromptParagraph:
                        return prompt.paragraphs[promptParagraphIndex].paragraphEvent;
                    case DialogueEventType.PromptResponse:
                        return prompt.promptResponses[promptResponseIndex].responseEvent;
                    default:
                        return null;
                }
            }
        }

        public void SetDialogueEvent(Dialogue dialogue, DialogueEventType dialogueEventType)
        {
            if(dialogueEventType != DialogueEventType.StartDialogue && dialogueEventType != DialogueEventType.EndDialogue)
            {
                Debug.LogError("DialogueEventType must be StartDialogue or EndDialogue when setting DialogueEvent from Dialogue");
                return;
            }

            this.dialogue = dialogue;
            eventType = dialogueEventType;
        }

        public void SetDialogueEvent(Prompt prompt, int index, DialogueEventType dialogueEventType)
        {
            if (dialogueEventType == DialogueEventType.StartDialogue && dialogueEventType == DialogueEventType.EndDialogue)
            {
                Debug.LogError("DialogueEventType must be PromptParagraph or PromptResponse when setting DialogueEvent from Prompt");
                return;
            }

            
            switch (dialogueEventType)
            {
                case DialogueEventType.PromptParagraph:
                    if (index < 0 || index >= prompt.paragraphs.Count)
                    {
                        Debug.LogError("Index out of range for PromptParagraph");
                        return;
                    }
                    promptParagraphIndex = index;
                    break;
                case DialogueEventType.PromptResponse:
                    if (index < 0 || index >= prompt.promptResponses.Count)
                    {
                        Debug.LogError("Index out of range for PromptResponse");
                        return;
                    }
                    promptResponseIndex = index;
                    break;
            }

            this.prompt = prompt;
            eventType = dialogueEventType;
        }



        public void Register()
        {
            DialogueEvent.RegisterListener(this);
        }

        public void Unregister()
        {
            DialogueEvent.UnregisterListener(this);
        }   

        public void OnEventRaised()
        {
            Event?.Invoke();
        }
    }

    public enum DialogueEventType
    {
        StartDialogue,
        EndDialogue,
        PromptParagraph,
        PromptResponse,

    }
}


