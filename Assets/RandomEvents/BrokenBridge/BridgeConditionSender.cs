using DialogueSystem;
using UnityEngine;

namespace RandomEvents.BrokenBridge
{
    public class BridgeConditionSender : MonoBehaviour
    {
        public PromptResponseConditionSender safeBridgeConditionSender;
        public PromptResponseConditionSender unsafeBridgeConditionSender;

        private void Start()
        {
            safeBridgeConditionSender.Register();
            unsafeBridgeConditionSender.Register();
        }

        private void OnDestroy()
        {
            safeBridgeConditionSender.Unregister();
            unsafeBridgeConditionSender.Unregister();
        }

        public bool Has1000GoldOrMore()
        {
            return ResourceManager.Instance != null && ResourceManager.Instance.Gold >= 1000;
        }
    }

}

