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

        public DialogueEvent startDialogueEvent;
        public DialogueEvent endDialogueEvent;

        public Dialogue nextDialogue;
    }

}
