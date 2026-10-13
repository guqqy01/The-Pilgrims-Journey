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

        [SerializeField] private Image rewardImage;
        [SerializeField] private TextMeshProUGUI rewardNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private float spriteSize = 50f;

        public void Setup(ILevelingUpReward reward, int levelsIncrease)
        {
            this.reward = reward;
            rewardNameText.text = reward.Name;
            descriptionText.text = reward.Description(levelsIncrease);
            rewardImage.sprite = reward.Sprite;
            rewardImage.NativeRatioScaleToSize(spriteSize);
        }

        public void Select()
        {
            LevelingUpRewardUIManager.IfReady(m => m.SelectReward(Reward));
        }
    }
}
