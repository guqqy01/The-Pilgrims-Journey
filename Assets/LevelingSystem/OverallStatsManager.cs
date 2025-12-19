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


        public IntOrFloatObject AddBuff(string reference, Buff buff, OverallStats stat)
        {
            IntOrFloatObject value;

            switch (stat)
            {
                case OverallStats.AmountOfRelicToChooseFrom:
                    value = amountOfRelicToChooseFrom.AddBuff(reference, buff);
                    break;
                case OverallStats.Luck:
                    value = luck.AddBuff(reference, buff);
                    break;
                case OverallStats.ExperienceGain:
                    value = experienceGain.AddBuff(reference, buff);
                    break;
                default:
                    throw new NotImplementedException();
            }

            buffsChanged?.Invoke();

            return value;
        }

        public IntOrFloatObject RemoveBuff(string reference, OverallStats stat)
        {
            IntOrFloatObject value;

            switch (stat)
            {
                case OverallStats.AmountOfRelicToChooseFrom:
                    value = amountOfRelicToChooseFrom.RemoveBuff(reference);
                    break;
                case OverallStats.Luck:
                    value = luck.RemoveBuff(reference);
                    break;
                case OverallStats.ExperienceGain:
                    value = experienceGain.RemoveBuff(reference);
                    break;
                default:
                    throw new NotImplementedException();
            }

            buffsChanged?.Invoke();

            return value;
        }
    }

    public enum OverallStats
    {
        AmountOfRelicToChooseFrom,
        Luck,
        ExperienceGain,
    }

}
