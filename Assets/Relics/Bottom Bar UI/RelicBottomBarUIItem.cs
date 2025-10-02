using Relics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Relics
{
    public class RelicBottomBarUIItem : MonoBehaviour
    {
        private RelicBottomBarUIManager manager;

        private Relic relic;

        [SerializeField]
        private Image relicImage;
        [SerializeField]
        private Text textTop;
        [SerializeField]
        private Text textBottom;

        public void Init(Relic relic, RelicBottomBarUIManager relicUIManager)
        {
            this.manager = relicUIManager;
            this.relic = relic;
            UpdateUI();

            foreach (RelicEffect effect in relic.relicEffects)
            {
                effect.updateUI += UpdateUI;
            }
        }


        public void Unsubscribe()
        {
            foreach (RelicEffect effect in relic.relicEffects)
            {
                effect.updateUI -= UpdateUI;
            }
        }

        public void UpdateUI()
        {
            relicImage.sprite = relic.relicSO.sprite;
            relicImage.SetNativeSize();
            relicImage.ScaleToSize(manager.RelicImageMaxSize);
            textTop.text = relic.TopText;
            textBottom.text = relic.BottomText;
        }
    }

}
