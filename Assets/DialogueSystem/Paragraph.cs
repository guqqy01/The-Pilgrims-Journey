using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace DialogueSystem
{
    [Serializable]
    public class Paragraph
    {
        [TextArea(5, 10)]
        [FormerlySerializedAs("promptText")]
        public string paragraph;
        public DialogueEvent paragraphEvent;
        public AudioDialogue audioDialogue;
    }
}