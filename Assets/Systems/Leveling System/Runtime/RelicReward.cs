using UnityEngine;
using Relics;

namespace LevelingSystem
{
    public class RelicReward : ILevelingUpReward
    {
        public RelicSO RelicSO { get; }

        public ScriptableObject Source => RelicSO;

        public Sprite Sprite => RelicSO.Sprite;

        public string Name => RelicSO.name;

        public string Description(int levelsIncrease)
        {
            string relicMainDescription = GetRelicStatIncrease(levelsIncrease);

            if (RelicManager.HasInstance)
                return relicMainDescription;

            relicMainDescription = relicMainDescription.AddStringWithCarriageReturn(RelicSO.description, 2);

            return relicMainDescription;
        }

        private string GetRelicStatIncrease(int levelsIncrease)
        {
            StatIncreaseRelicEffect statIncreaseEffect =
                RelicSO.relicEffects.Find(x => x is StatIncreaseRelicEffect)
                as StatIncreaseRelicEffect;

            if (statIncreaseEffect == null)
            {
                return string.Empty;
            }
            string returnString = string.Empty;

            int currentLevel = 0;

            if (RelicManager.Instance.TryGetRelic(RelicSO, out Relic relic))
                currentLevel = relic.Level;

            Buff fromBuff = statIncreaseEffect.BuffIncreasePerLevel * currentLevel;
            Buff toBuff = statIncreaseEffect.BuffIncreasePerLevel * (currentLevel + levelsIncrease);

            returnString =
                $"{statIncreaseEffect.StatString}: {Buff.SimpleCompareString(fromBuff, toBuff)}";

            return returnString;
        }

        public RelicReward(RelicSO relicSO)
        {
            RelicSO = relicSO;
        }

        public void Apply(int amount)
        {
            if (RelicManager.Instance.TryGetRelic(RelicSO, out Relic relic))
            {
                relic.Level += amount;
            }
            else
            {
                RelicManager.Instance.AddRelic(RelicSO, amount);
            }
        }
    }
}

