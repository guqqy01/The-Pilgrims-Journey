using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace DialogueSystem
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField]
        private Dialogue startDialogue;

        [SerializeField]
        private TextMeshProUGUI mainText;

        [SerializeField]
        private PromptResponseButton dialogueButtonPrefab;
        private List<PromptResponseButton> dialogueButtons = new List<PromptResponseButton>();

        [SerializeField]
        private Transform dialogueButtonsParentTransform;

        [SerializeField]
        private GameObject historyGO;

        [SerializeField]
        private DialogueHistory history;

        [SerializeField]
        private float wordWriteSpeed = 30;

        private Dialogue currentDialogue;
        public Dialogue CurrentDialogue
        {
            get { return currentDialogue; }
            set { currentDialogue = value; }
        }

        private Prompt currentPrompt;
        public Prompt CurrentPrompt
        {
            get { return currentPrompt; }
            set { currentPrompt = value; }
        }
        private Coroutine currentCoroutine;

        private bool finishedWritingPromptItem;

        [SerializeField]
        private AudioDialoguePlayer audioDialoguePlayer;

        private void Start()
        {
            if (startDialogue != null)
            {
                SetupDialogue(startDialogue);
            }
        }

        private void CloseDialogue()
        {
            currentDialogue = null;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Return))
                return;


            Skip();
        }

        public void Skip()
        {
            if (!finishedWritingPromptItem)
            {
                FinishWritingPrompt();
                return;
            }

            if (!currentPrompt.OnLastPrompt)
            {
                currentPrompt.Next();
                return;
            }

            if (currentPrompt.promptResponses.Count > 0)
                return;

            if (currentPrompt.nextPrompt != null)
            {
                StartNewPrompt(currentPrompt.nextPrompt, NextPromptSetupSettings.GoToStart);
            }
            else
            {
                CompleteCurrentDialogue();
            }
        }

        public void CompleteCurrentDialogue()
        {
            currentDialogue?.endDialogueEvent.Raise("End Dialogue");

            if (currentDialogue != null && currentDialogue.nextDialogue != null)
            {
                SetupDialogue(currentDialogue.nextDialogue);
            }
            else
            {
                CloseDialogue();
            }
        }

        public void SetupDialogue(Dialogue dialogue)
        {
            currentDialogue = dialogue;

            currentDialogue.initialPrompt.Initialize();

            currentDialogue?.startDialogueEvent.Raise("Start Dialogue");

            StartNewPrompt(dialogue.initialPrompt, dialogue.settings);
        }

        public void FinishWritingPrompt()
        {
            if(currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            mainText.text = currentPrompt.CurrentText;
            AddDialogueHistory(currentPrompt.CurrentText);
            dialogueButtons.DestroyAndClearList();

            if (currentPrompt.OnLastPrompt)
            {
                for (int i = 0; i < currentPrompt.promptResponses.Count; i++)
                {
                    PromptResponseButton button = Instantiate(dialogueButtonPrefab, dialogueButtonsParentTransform);
                    button.Setup(this, currentPrompt.promptResponses[i]);
                    dialogueButtons.Add(button);
                }
            }

            finishedWritingPromptItem = true;
        }

        public void StartNewPrompt(Prompt prompt, NextPromptSetupSettings nextPromptSetupSettings)
        {
            switch (nextPromptSetupSettings)
            {
                case NextPromptSetupSettings.Reset:
                    prompt.Initialize();
                    break;
                case NextPromptSetupSettings.GoToStart:
                    prompt.GoToStart();
                    break;
                case NextPromptSetupSettings.GoToEnd:
                    prompt.GoToEnd();
                    break;
            }


            prompt.onPageChange = NextPrompItem;

            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            currentPrompt = prompt;
            currentCoroutine = StartCoroutine(WritePromptItem(prompt.CurrentPromptParagraph));
        }

        private void NextPrompItem(Paragraph item)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(WritePromptItem(item));
        }

        private IEnumerator WritePromptItem(Paragraph paragraph)
        {
            mainText.text = "";
            dialogueButtons.DestroyAndClearList();
            finishedWritingPromptItem = false;

            audioDialoguePlayer.Play(paragraph.audioDialogue);

            for (int i = 0; i < paragraph.paragraph.Length; i++)
            {
                yield return new WaitForSeconds(1f / wordWriteSpeed);
                mainText.text += paragraph.paragraph[i];
            }

            finishedWritingPromptItem = true;

            AddDialogueHistory(paragraph.paragraph);

            if (currentPrompt.OnLastPrompt)
            {
                yield return new WaitForSeconds(0.5f);
                WritePromptResponses(currentPrompt.promptResponses);
            }
        }

        private void WritePromptResponses(List<PromptResponse> promptResponses)
        {
            for (int i = 0; i < promptResponses.Count; i++)
            {
                PromptResponseButton button = Instantiate(dialogueButtonPrefab, dialogueButtonsParentTransform);
                button.Setup(this, promptResponses[i]);
                dialogueButtons.Add(button);
            }
        }

        public void OpenCloseHistory()
        {
            historyGO.SetActive(!historyGO.activeInHierarchy);
        }

        public void AddDialogueHistory(string text)
        {
            history.AddHistory(text);
        }
    }
}