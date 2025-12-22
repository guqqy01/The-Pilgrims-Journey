using UnityEngine;
using System.Collections.Generic;
using Relics;

namespace LevelingSystem
{
    public class LevelingUpRewardManager : Singleton<LevelingUpRewardManager>
    {
        public delegate void OnShowUI(Dictionary<RelicSO, int> relicSOs);
        public OnShowUI onShowUI;

        [SerializeField]
        private WeightedChanceList<RelicSO> unitStatRelics;
        [SerializeField]
        private WeightedChanceList<RelicSO> heroStatRelics;
        [SerializeField]
        private WeightedChanceList<RelicSO> heroWeaponRelics;

        private readonly WeightedChanceList<WeightedChanceList<RelicSO>> relicLists = new WeightedChanceList<WeightedChanceList<RelicSO>>();


        public Dictionary<RelicSO, int> RelicsOfferedAndAmountOfLevelsGained { private set; get; } = new Dictionary<RelicSO, int>();

        private void Awake()
        {
            relicLists.Add(unitStatRelics, 1);
            relicLists.Add(heroStatRelics, 1);
            relicLists.Add(heroWeaponRelics, 1);
        }

        protected override void CleanUp()
        {
            onShowUI = null;
        }

        public void SetupRelicsChoices()
        {
            RelicsOfferedAndAmountOfLevelsGained.Clear();

            WeightedChanceList<RelicSO> randomRelicRewards = relicLists.GetRandom();

            foreach (WeightedChanceObject<WeightedChanceList<RelicSO>> list in relicLists.weightedList)
            {
                if(list.item == randomRelicRewards)
                {
                    relicLists[list.item] = 1;
                }
                else
                {
                    relicLists[list.item] += 1;
                }
            }

            while (RelicsOfferedAndAmountOfLevelsGained.Count < OverallStatsManager.Instance.AmountOfRelicToChooseFrom && RelicsOfferedAndAmountOfLevelsGained.Count < unitStatRelics.weightedList.Count)
            {
                RelicSO relicSO = unitStatRelics.GetRandom();

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
            float luck = Mathf.Log10(Mathf.Max(OverallStatsManager.Instance.Luck, 0) + 1);

            //Standard deviation on the curve
            float standardDiv = 2f;

            //random position on the curve, within 0 and standardDiv * (Sqrt(2 * PI)).
            float y = Random.Range(Mathf.Epsilon, (1f / (standardDiv * Mathf.Sqrt(2 * Mathf.PI))) - Mathf.Epsilon);

            //Calculate one side of the curve;
            float standardDivCurve = standardDiv * (Mathf.Sqrt(2f)) * Mathf.Sqrt(Mathf.Log10(1f / (standardDiv * Mathf.Sqrt(2 * Mathf.PI) * y)));

            //boolean of wether to sample sample from the left or right side of the curve.
            int rightSide = Random.Range(0, 2);

            int value;

            if (rightSide >= 1)
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
            if (RelicManager.Instance.TryGetRelic(relicSO, out Relic relic))
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
}

