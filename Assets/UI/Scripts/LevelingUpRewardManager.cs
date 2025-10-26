using UnityEngine;
using System.Collections.Generic;
using Relics;
using UnityEngine.UI;
public class LevelingUpRewardManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static LevelingUpRewardManager Instance { private set; get; }

    public delegate void OnShowUI(Dictionary<RelicSO, int> relicSOs);
    public OnShowUI onShowUI;

    [SerializeField]
    private WeightedChanceList<RelicSO> relicsAbleToRecieveThroughLeveling;

    public Dictionary<RelicSO, int> RelicsOfferedAndAmountOfLevelsGained { private set; get; } = new Dictionary<RelicSO, int>();

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
        RelicsOfferedAndAmountOfLevelsGained.Clear();

        while (RelicsOfferedAndAmountOfLevelsGained.Count < OverallBuffManager.Instance.AmountOfRelicToChooseFrom && RelicsOfferedAndAmountOfLevelsGained.Count < relicsAbleToRecieveThroughLeveling.weightedList.Count)
        {
            RelicSO relicSO = relicsAbleToRecieveThroughLeveling.GetRandom();

            if (!RelicsOfferedAndAmountOfLevelsGained.ContainsKey(relicSO))
            {
                RelicsOfferedAndAmountOfLevelsGained.Add(relicSO, GetAmountOfLevelsGained());
            }
        }

        onShowUI?.Invoke(RelicsOfferedAndAmountOfLevelsGained);

        Time.timeScale = 0f;
    }

    private int GetAmountOfLevelsGained()
    {
        //luck is where on the xAxis the standard deviation curve lies, its it Mathf.Log10'd to stop exponential boosts.
        float luck = Mathf.Log10(Mathf.Max(OverallBuffManager.Instance.Luck, 0) + 1);

        //Standard deviation on the curve
        float standardDiv = 2f;

        //random position on the curve, within 0 and standardDiv * (Sqrt(2 * PI)).
        float y = Random.Range(Mathf.Epsilon, (1f / (standardDiv * Mathf.Sqrt(2 * Mathf.PI))) - Mathf.Epsilon);

        //Calculate one side of the curve;
        float standardDivCurve = standardDiv * (Mathf.Sqrt(2f)) * Mathf.Sqrt(Mathf.Log10(1f / (standardDiv * Mathf.Sqrt(2 * Mathf.PI) * y)));

        //boolean of wether to sample sample from the left or right side of the curve.
        int rightSide = Random.Range(0, 2);

        int value;

        if(rightSide >= 1)
        {
            int rightSideOfCurve = Mathf.RoundToInt(luck + standardDivCurve);
            value = rightSideOfCurve;
        }
        else
        {
            int leftSideOfCurve = Mathf.RoundToInt(luck - standardDivCurve);
            value = leftSideOfCurve;
        }

        //Return at least one level increase.
        return Mathf.Max(value, 1);
    }

    public void SelectRelic(RelicSO relicSO)
    {
        if(RelicManager.Instance.TryGetRelic(relicSO, out Relic relic))
        {
            relic.Level += RelicsOfferedAndAmountOfLevelsGained[relicSO];
        }
        else
        {
            RelicManager.Instance.AddRelic(relicSO, RelicsOfferedAndAmountOfLevelsGained[relicSO]);
        }

        Time.timeScale = 1f;
    }
}
