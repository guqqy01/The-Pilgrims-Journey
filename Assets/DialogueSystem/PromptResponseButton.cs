using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem
{
    public class PromptResponseButton : MonoBehaviour
    {
        public DialogueManager DialogueManager { private set; get; }

        private PromptResponse response;

        [SerializeField]
        private Text responseText;

        [SerializeField]
        private Text indentText;

        [SerializeField]
        private Color goneDownPathColor = Color.grey;

        public void OnClick()
        {
            response.GoneDownPromptPath = true;
            DialogueManager.AddDialogueHistory(response.response);
            response.responseEvent.Raise();

            if (response.nextPrompt != null)
            {
                DialogueManager.StartNewPrompt(response.nextPrompt, response.nextPromptSetupSettings);
            }
            else if(DialogueManager.CurrentPrompt.nextPrompt != null)
            {
                DialogueManager.StartNewPrompt(DialogueManager.CurrentPrompt.nextPrompt, NextPromptSetupSettings.Reset);
            }
            else
            {
                DialogueManager.CompleteCurrentDialogue();
            }
        }

        public void Setup(DialogueManager dialogueManager, PromptResponse response)
        {
            this.response = response;
            DialogueManager = dialogueManager;
            responseText.text = response.response;

            responseText.color = response.GoneDownPromptPath ? goneDownPathColor : Color.white;
        }
    }

}
