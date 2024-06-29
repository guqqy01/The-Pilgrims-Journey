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

        public UnityEvent startDialogueAction;
        public UnityEvent endDialogueAction;

        public Dialogue nextDialogue;
    }

}
