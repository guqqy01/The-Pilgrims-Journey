using UnityEngine;
using DialogueSystem;

namespace RandomEvents
{
    [CreateAssetMenu()]
    public class RandomEvent : ScriptableObject
    {
        public Sprite backgroundSprite;
        public Dialogue dialogue;
        public bool isRepeatable = true;
    }
}
