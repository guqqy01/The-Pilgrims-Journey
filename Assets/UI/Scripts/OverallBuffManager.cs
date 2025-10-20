using Units;
using UnityEngine;

public class OverallBuffManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static OverallBuffManager Instance { private set; get; }

    public BuffableStat Luck;
    public BuffableStat Health;
    public BuffableStat Shield;


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
