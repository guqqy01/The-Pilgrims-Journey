using DialogueSystem;
using UnityEngine;

public class DemonPromptListener : MonoBehaviour
{
    [SerializeField]
    private DialogueEventListener acceptTheGiftListener;

    [SerializeField]
    private DialogueEventListener refuseTheGiftListener;

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
        Debug.Log("Accepted the gift");
    }

    public void RefuseTheGift()
    {
        Debug.Log("Refused the gift");
    }
}
