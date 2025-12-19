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
            LevelingUpRewardManager.Instance.SetupRelicsChoices();
        }

        private void OnUnitPassed(int x, int y)
        {
            CurrentExperience = CurrentExperience + OverallStatsManager.Instance.ExperienceGain;

            if (CurrentExperience >= ExperienceNeededToLevel)
            {
                LevelUp();
            }

            onUpdateUI?.Invoke();
        }
    }

}

