using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DialogueSystem
{
    [CreateAssetMenu(menuName = "DialogueSystem/Prompt")]
    public class Prompt : ScriptableObject
    {
        [FormerlySerializedAs("prompt")]
        public List<Paragraph> paragraphs;

        public List<PromptResponse> promptResponses;

        public Prompt nextPrompt;

        public bool OnLastPrompt => currentIndex == paragraphs.Count - 1;

        public delegate void OnPageChange(Paragraph item);
        public OnPageChange onPageChange;


        private int currentIndex = -1;
        public int CurrentIndex
        {
            set
            {
                int previous = currentIndex;
                currentIndex = Mathf.Clamp(value, 0, paragraphs.Count - 1);
                if (currentIndex != previous)
                {
                    onPageChange?.Invoke(paragraphs[currentIndex]);
                    CurrentPromptParagraph.paragraphEvent.Raise();
                }
            }
            get
            {
                return currentIndex;
            }
        }



        public Paragraph CurrentPromptParagraph
        {
            get
            {
                if (paragraphs.Count >= 1)
                    return paragraphs[currentIndex];

                return null;
            }
        }

        public string CurrentText
        {
            get
            {
                if (paragraphs.Count >= 1)
                    return paragraphs[currentIndex].paragraph;

                return string.Empty;
            }
        }

        

        
        public void Next()
        {
            CurrentIndex++;
        }

        public void Initialize()
        {
            GoToStart();

            foreach (var promptResponse in promptResponses)
            {
                promptResponse.GoneDownPromptPath = false;
            }
        }

        public void GoToStart()
        {
            currentIndex = 0;
        }

        public void GoToEnd()
        {
            currentIndex = paragraphs.Count - 1;
        }
    }

}
