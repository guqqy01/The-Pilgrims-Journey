using UnityEngine;

namespace LevelingSystem
{

    public class OverallBuffManager : Singleton<OverallBuffManager>
    {
        //Leveling Up
        public BuffableStat AmountOfRelicToChooseFrom = new BuffableStat(3);
        public BuffableStat Luck = new BuffableStat(0f);
        public BuffableStat ExperienceGain = new BuffableStat(1f);
    }

}
