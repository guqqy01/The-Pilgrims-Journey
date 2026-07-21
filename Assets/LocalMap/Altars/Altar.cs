using Heros;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Altar : MonoBehaviour, IInteractable
{
    [SerializeField]
    private int cost;

    [SerializeField]
    private AltarPayment altarPayment;

    public enum AltarPayment 
    { 
        Gold,
        Health,
        Combat,
    }

    public bool CanInteract()
    {
        switch (altarPayment)
        {
            case AltarPayment.Gold:
                return ResourceManager.Instance.Gold >= cost;
            case AltarPayment.Health:
                return HeroManager.Instance.ActiveHero.HealthAndShieldManager.Health.CurrentHealth >= cost;
            case AltarPayment.Combat:
                return true;
        }

        return true;
    }

    public bool Interact()
    {
        if(!CanInteract())
            return false;

        Debug.Log("Interacted");


        return true;
    }
}

public class AltarUI : MonoBehaviour
{
    [SerializeField]
    private Image itemImage;
    [SerializeField]
    private Image paymentImage;
    [SerializeField]
    private TextMeshProUGUI costText;
}
