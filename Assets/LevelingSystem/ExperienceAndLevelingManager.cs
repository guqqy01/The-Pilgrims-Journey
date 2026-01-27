using UnityEngine;

namespace LevelingSystem
{
    public class ExperienceAndLevelingManager : Singleton<ExperienceAndLevelingManager>
    {
        public int Level { private set; get; } = 0;
        public float CurrentExperience { private set; get; }
        public float ExperienceNeededToLevel => Level * 10f + 10f;

        public delegate void OnUpdateUI();
        public OnUpdateUI onUpdateUI;


        private void Awake()
        {
            LocalMapLevelCompletedManager.onUnitPassed += OnUnitPassed;
        }

        protected override void CleanUp()
        {
            LocalMapLevelCompletedManager.onUnitPassed -= OnUnitPassed;
        }

        public void LevelUp()
        {
            Level++;
            CurrentExperience = 0;
            LevelingUpRewardManager.Instance.SetupRewardChoices();
        }

        public void AddExperience(float amount)
        {
            float modifiedAmount = OverallStatsManager.Instance.ExperienceGainBuff.Value(amount);

            CurrentExperience += modifiedAmount;
            if (CurrentExperience >= ExperienceNeededToLevel)
            {
                LevelUp();
            }
            onUpdateUI?.Invoke();
        }

        private void OnUnitPassed(int x, int y)
        {
            AddExperience(1);
        }
    }

}

