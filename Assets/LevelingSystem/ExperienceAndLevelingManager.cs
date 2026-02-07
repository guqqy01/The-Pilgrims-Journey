using GameManagerCalls;
using LocalMap.UI;
using System;
using UnityEngine;

namespace LevelingSystem
{
    public class ExperienceAndLevelingManager : Singleton<ExperienceAndLevelingManager>, ISaveLoad, IOnSave, IOnLoad, IOnClear
    {
        public int Level { private set; get; } = 0;
        public float CurrentExperience { private set; get; }
        public float ExperienceNeededToLevel => Level * 10f + 10f;

        public string SaveFileName => "ExperienceAndLevelManager";

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

        public void Save()
        {
            SaveManager.IfFileExistsDelete(SaveFileName);
            SaveData saveData = new SaveData(Level, CurrentExperience);
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.SaveOver(saveProfile);
        }

        public void Load()
        {
            if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
            {
                SaveData saveData = outSaveProfile.saveData;

                Level = saveData.level;
                CurrentExperience = saveData.currentExperience;
            }
            else
            {
                Debug.LogWarning($"No save data found for {SaveFileName}");
                return;
            }
        }

        public void Clear()
        {
            Level = 0;
            CurrentExperience = 0;
        }

        [Serializable]
        private class SaveData : SaveProfileData
        {
            public int level;
            public float currentExperience;

            public SaveData(int level, float currentExperience)
            {
                this.level = level;
                this.currentExperience = currentExperience;
            }
        }
    }

}

