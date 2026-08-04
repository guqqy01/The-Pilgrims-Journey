using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LevelingSystem;
using Relics;

namespace LevelingSystem.UI
{
    public class LevelingUpRewardUIItem : MonoBehaviour
    {
        private ILevelingUpReward reward;
        public ILevelingUpReward Reward => reward;
        private int levelsIncrease;

        [SerializeField] private Image rewardImage;
        [SerializeField] private TextMeshProUGUI rewardNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private float spriteSize = 50f;

        public void Setup(ILevelingUpReward reward, int levelsIncrease)
        {
            this.reward = reward;
            this.levelsIncrease = levelsIncrease;

            rewardNameText.text = reward.Name;
            descriptionText.text = reward.Description(levelsIncrease);
            rewardImage.sprite = reward.Sprite;
            rewardImage.NativeRatioScaleToSize(spriteSize);

            /*if (source is RelicSO relicSO)
            {
                SetupRelicUI(relicSO);
            }
            else
            {
                SetupGenericRewardUI(source);
            }*/
        }

        // -------------------------
        // RELIC-SPECIFIC UI
        // -------------------------
        private void SetupRelicUI(RelicSO relicSO)
        {
            StatIncreaseRelicEffect statIncreaseEffect =
                relicSO.relicEffects.Find(x => x is StatIncreaseRelicEffect)
                as StatIncreaseRelicEffect;

            if (statIncreaseEffect == null)
            {
                descriptionText.text = string.Empty;
                return;
            }

            int currentLevel = 0;

            if (RelicManager.Instance.TryGetRelic(relicSO, out Relic relic))
                currentLevel = relic.Level;

            Buff fromBuff = statIncreaseEffect.BuffIncreasePerLevel * currentLevel;
            Buff toBuff = statIncreaseEffect.BuffIncreasePerLevel * (currentLevel + levelsIncrease);

            descriptionText.text =
                $"{statIncreaseEffect.StatString}: {Buff.SimpleCompareString(fromBuff, toBuff)}";
        }

        // -------------------------
        // FALLBACK FOR NON-RELIC REWARDS
        // -------------------------
        private void SetupGenericRewardUI(ScriptableObject source)
        {
            descriptionText.text = $"+{levelsIncrease}";
        }
    }
}
