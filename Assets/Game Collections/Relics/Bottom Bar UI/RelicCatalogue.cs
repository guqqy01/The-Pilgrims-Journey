using Monsters;
using Relics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Relics
{
    public class RelicCatalogue : MonoBehaviour
    {
        public RelicManager manager;

        [SerializeField]
        private List<RelicSO> allRelics;

        [SerializeField]
        private Image spriteImage;
        [SerializeField]
        private Text nameText;
        [SerializeField]
        private Text descriptionText;

        private RelicSO relic;

        private int index;

        private void Start()
        {
            manager = RelicManager.Instance;

            UpdateUI(allRelics[index]);
        }

        public void LeftButtonClick()
        {
            index = Mathf.Clamp(index - 1, 0, allRelics.Count - 1);

            UpdateUI(allRelics[index]);
        }

        public void RightButtonClick()
        {
            index = Mathf.Clamp(index + 1, 0, allRelics.Count - 1);

            UpdateUI(allRelics[index]);
        }

        private void UpdateUI(RelicSO relic)
        {
            spriteImage.sprite = relic.sprite;
            spriteImage.SetNativeSize();
            spriteImage.ScaleToSize(50f);
            nameText.text = relic.name;
            descriptionText.text = relic.description;
        }
    }

}
