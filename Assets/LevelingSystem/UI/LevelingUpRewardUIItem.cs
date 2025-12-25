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
        private int levelsIncrease;

        [SerializeField] private Image relicImage;
        [SerializeField] private TextMeshProUGUI relicName;
        [SerializeField] private TextMeshProUGUI statIncrease;
        [SerializeField] private float spriteSize = 50f;

        public System.Action<ILevelingUpReward> onButtonClick;

        public void ButtonClick()
        {
            onButtonClick?.Invoke(reward);
        }

        private void OnDestroy()
        {
            onButtonClick = null;
        }

        public void Setup(ILevelingUpReward reward, int levelsIncrease)
        {
            this.reward = reward;
            this.levelsIncrease = levelsIncrease;

            // All rewards must expose a source SO
            ScriptableObject source = reward.Source;

            relicName.text = source.name;

            if (source is RelicSO relicSO)
            {
                SetupRelicUI(relicSO);
            }
            else
            {
                SetupGenericRewardUI(source);
            }
        }

        // -------------------------
        // RELIC-SPECIFIC UI
        // -------------------------
        private void SetupRelicUI(RelicSO relicSO)
        {
            relicImage.sprite = relicSO.sprite;
            relicImage.NativeRatioScaleToSize(spriteSize);

            StatIncreaseRelicEffect statIncreaseEffect =
                relicSO.relicEffects.Find(x => x is StatIncreaseRelicEffect)
                as StatIncreaseRelicEffect;

            if (statIncreaseEffect == null)
            {
                statIncrease.text = string.Empty;
                return;
            }

            int currentLevel = 0;

            if (RelicManager.Instance.TryGetRelic(relicSO, out Relic relic))
                currentLevel = relic.Level;

            Buff fromBuff = statIncreaseEffect.BuffIncreasePerLevel * currentLevel;
            Buff toBuff = statIncreaseEffect.BuffIncreasePerLevel * (currentLevel + levelsIncrease);

            statIncrease.text =
                $"{statIncreaseEffect.StatString}: {Buff.SimpleCompareString(fromBuff, toBuff)}";
        }

        // -------------------------
        // FALLBACK FOR NON-RELIC REWARDS
        // -------------------------
        private void SetupGenericRewardUI(ScriptableObject source)
        {
            statIncrease.text = $"+{levelsIncrease}";

            if (relicImage != null)
                relicImage.enabled = false;
        }
    }
}
