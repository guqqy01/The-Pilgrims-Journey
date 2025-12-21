using Heros;
using System;
using UnityEngine;

namespace LevelingSystem
{

    public class OverallStatsManager : Singleton<OverallStatsManager>
    {
        private BuffableStat amountOfRelicToChooseFrom = new BuffableStat(3);
        private BuffableStat luck = new BuffableStat(0f);
        private BuffableStat experienceGain = new BuffableStat(1f);

        //Leveling Up
        public IntOrFloatObject AmountOfRelicToChooseFrom => amountOfRelicToChooseFrom.Value;
        public IntOrFloatObject Luck => luck.Value;
        public IntOrFloatObject ExperienceGain => experienceGain.Value;

        public delegate void OnBuffsChanged();
        public OnBuffsChanged buffsChanged;


        public void AddBuff(string reference, Buff buff, OverallStats stat)
        {
            switch (stat)
            {
                case OverallStats.AmountOfRelicToChooseFrom:
                    amountOfRelicToChooseFrom.AddBuff(reference, buff);
                    break;
                case OverallStats.Luck:
                    luck.AddBuff(reference, buff);
                    break;
                case OverallStats.ExperienceGain:
                    experienceGain.AddBuff(reference, buff);
                    break;
                default:
                    throw new NotImplementedException();
            }

            buffsChanged?.Invoke();
        }

        public void RemoveBuff(string reference, OverallStats stat)
        {
            switch (stat)
            {
                case OverallStats.AmountOfRelicToChooseFrom:
                    amountOfRelicToChooseFrom.RemoveBuff(reference);
                    break;
                case OverallStats.Luck:
                    luck.RemoveBuff(reference);
                    break;
                case OverallStats.ExperienceGain:
                    experienceGain.RemoveBuff(reference);
                    break;
                default:
                    throw new NotImplementedException();
            }

            buffsChanged?.Invoke();
        }
    }

    public enum OverallStats
    {
        AmountOfRelicToChooseFrom,
        Luck,
        ExperienceGain,
    }

}
