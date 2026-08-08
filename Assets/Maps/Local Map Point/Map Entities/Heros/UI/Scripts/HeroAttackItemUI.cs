using Heros;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Attacks.UI
{
    public class HeroAttackItemUI : MonoBehaviour
    {
        [SerializeField]
        private Image itemImage;

        [SerializeField]
        private Transform levelsHolderTransform;

        [SerializeField]
        private Image levelPrefab;

        private List<Image> levelPrefabs = new List<Image>();

        [SerializeField]
        private Sprite leveledSprite;
        [SerializeField]
        private Sprite unleveledSprite;

        public void UpdateUI(BaseAttack heroAttack)
        {
            BaseAttackSO heroAttackSO = heroAttack.BaseAttackSO;

            itemImage.sprite = heroAttack.BaseAttackSO.icon;

            levelPrefabs.DestroyAndClearList();

            for (int i = 0; i < heroAttack.Level; i++)
            {
                Image spawnedLevelImage = Instantiate(levelPrefab, levelsHolderTransform);
                spawnedLevelImage.sprite = leveledSprite;
                levelPrefabs.Add(spawnedLevelImage);
            }

            for (int i = 0; i < heroAttackSO.MaxLevel - heroAttack.Level; i++)
            {
                Image spawnedLevelImage = Instantiate(levelPrefab, levelsHolderTransform);
                spawnedLevelImage.sprite = unleveledSprite;
                levelPrefabs.Add(spawnedLevelImage);
            }
        }
    }

}
