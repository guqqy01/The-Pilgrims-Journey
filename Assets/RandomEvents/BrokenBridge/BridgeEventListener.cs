using DialogueSystem;
using UnityEngine;
using UnityEngine.UI;
public class BridgeEventListener : MonoBehaviour
{
    [SerializeField]
    private DialogueEventListener repairTheBridgeListener;

    [SerializeField]
    private DialogueEventListener riskItSafeListener;

    [SerializeField]
    private DialogueEventListener riskItUnsafeListener;

    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Sprite repairTheBridgeSprite;
    [SerializeField]
    private Sprite riskItSafeSprite;
    [SerializeField]
    private Sprite riskItUnsafeSprite;

    private void OnEnable()
    {
        repairTheBridgeListener.Register();
        riskItSafeListener.Register();
        riskItUnsafeListener.Register();
    }

    private void OnDisable()
    {
        repairTheBridgeListener.Unregister();
        riskItSafeListener.Unregister();
        riskItUnsafeListener.Unregister();
    }

    public void RepairTheBridge()
    {
        ResourceManager.Instance.ModifyGold(-1000);

        ChangeRandomBackgroundImage.Instance.ChangeBackground(repairTheBridgeSprite);
    }

    public void RiskItSafe()
    {
        ChangeRandomBackgroundImage.Instance.ChangeBackground(riskItSafeSprite);
    }

    public void RiskItUnsafe()
    {
        ChangeRandomBackgroundImage.Instance.ChangeBackground(riskItUnsafeSprite);
    }
}
