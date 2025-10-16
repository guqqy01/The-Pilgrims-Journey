using DialogueSystem;
using Relics;
using UnityEngine;

public class DemonPromptListener : MonoBehaviour
{
    [SerializeField]
    private DialogueEventListener acceptTheGiftListener;

    [SerializeField]
    private DialogueEventListener refuseTheGiftListener;

    [SerializeField]
    private RelicSO demonRelic;

    private void OnEnable()
    {
        acceptTheGiftListener.Register();
        refuseTheGiftListener.Register();
    }

    private void OnDisable()
    {
        acceptTheGiftListener.Unregister();
        refuseTheGiftListener.Unregister();
    }   

    public void AcceptTheGift()
    {
        RelicManager.Instance.AddRelic(demonRelic);
    }

    public void RefuseTheGift()
    {
        // Nothing happens
    }
}
