using UnityEngine;
using System.Collections.Generic;
using Relics;
public class LevelRewardManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static LevelRewardManager Instance { private set; get; }

    public delegate void OnShowUI();
    public OnShowUI onShowUI;

    [SerializeField]
    private WeightedChanceList<RelicSO> relicsAbleToRecieveThroughLeveling;

    public List<RelicSO> RelicsOffered { private set; get; }

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

    public void SetupRelicsChoices()
    {

    }
}
