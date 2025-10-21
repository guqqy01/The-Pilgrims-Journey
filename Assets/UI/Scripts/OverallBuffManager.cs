using Units;
using UnityEngine;

public class OverallBuffManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static OverallBuffManager Instance { private set; get; }


    public BuffableStat AmountOfRelicToChooseFrom = new BuffableStat(3);
    public BuffableStat Luck = new BuffableStat(0f);
    public BuffableStat Health = new BuffableStat(0);
    public BuffableStat Shield = new BuffableStat(0);
    public BuffableStat Movement = new BuffableStat(0f);
    public BuffableStat SpawnRate = new BuffableStat(0f);
    public BuffableStat ShieldCooldown = new BuffableStat(0f);
    public BuffableStat ShieldRegeneration = new BuffableStat(0f);

    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void Init()
    {

    }
}
