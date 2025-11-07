using Relics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LevelingSystem.UI
{
    public class LevelingUpRelicUIItem : MonoBehaviour
    {
        private RelicSO relicSO;

        [SerializeField]
        private Image relicImage;
        [SerializeField]
        private TextMeshProUGUI relicName;
        [SerializeField]
        private TextMeshProUGUI StatIncrease;
        [SerializeField]
        private float spriteSize = 50f;

        public delegate void OnButtonClick(RelicSO relic);
        public OnButtonClick onButtonClick;

        public void ButtonClick()
        {
            onButtonClick?.Invoke(relicSO);
        }

        private void OnDestroy()
        {
            onButtonClick = null;
        }

        public void Setup(RelicSO relicSO, int levelsIncrease)
        {
            this.relicSO = relicSO;

            relicImage.sprite = relicSO.sprite;
            relicImage.NativeRatioScaleToSize(spriteSize);

            relicName.text = relicSO.name;


            if (RelicManager.Instance.TryGetRelic(relicSO, out Relic relic))
            {
                StatIncreaseRelicEffect statIncreaseEffect = relicSO.relicEffects.Find(x => x is StatIncreaseRelicEffect) as StatIncreaseRelicEffect;

                if (statIncreaseEffect == null)
                    return;

                string statIncreaseString = statIncreaseEffect.StatString;

                Buff fromBuff = statIncreaseEffect.BuffIncreasePerLevel * relic.Level;
                Buff toBuff = statIncreaseEffect.BuffIncreasePerLevel * (relic.Level + levelsIncrease);

                statIncreaseString = statIncreaseString + ": " + Buff.SimpleCompareString(fromBuff, toBuff);

                StatIncrease.text = statIncreaseString;
            }
            else
            {
                StatIncreaseRelicEffect statIncreaseEffect = relicSO.relicEffects.Find(x => x is StatIncreaseRelicEffect) as StatIncreaseRelicEffect;

                if (statIncreaseEffect == null)
                    return;

                string statIncreaseString = statIncreaseEffect.StatString;

                Buff fromBuff = statIncreaseEffect.BuffIncreasePerLevel * 0f;
                Buff toBuff = statIncreaseEffect.BuffIncreasePerLevel * (levelsIncrease);

                statIncreaseString = statIncreaseString + ": " + Buff.SimpleCompareString(fromBuff, toBuff);

                StatIncrease.text = statIncreaseString;
            }
        }


    }
}

