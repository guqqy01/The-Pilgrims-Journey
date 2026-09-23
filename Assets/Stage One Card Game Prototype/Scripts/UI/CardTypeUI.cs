using TPJ.Gameplay;
using UnityEngine;
using UnityEngine.UI;
namespace TPJ.UI
{
    public class CardTypeUI : MonoBehaviour
    {
        [SerializeField]
        private Image cardTypeImage;

        [SerializeField]
        private Image activationType;

        [SerializeField]
        private Image dropShadowImage;

        [SerializeField]
        private Sprite farmerTypeSprite;
        [SerializeField]
        private Sprite banditTypeSprite;
        [SerializeField]
        private Sprite guardTypeSprite;
        [SerializeField]
        private Sprite scoutTypeSprite;
        [SerializeField]
        private Sprite travelerTypeSprite;
        [SerializeField]
        private Sprite brigadeTypeSprite;
        public void Setup(TileCardType cardType)
        {
            Sprite sprite = cardType switch
            {
                TileCardType.Farmer => farmerTypeSprite,
                TileCardType.Bandit => banditTypeSprite,
                TileCardType.Brigade => brigadeTypeSprite,
                TileCardType.Guard => guardTypeSprite,
                TileCardType.Scout => scoutTypeSprite,
                TileCardType.Traveler => travelerTypeSprite,
                _ => farmerTypeSprite
            };

            cardTypeImage.sprite = sprite;
            dropShadowImage.sprite = sprite;
        }
    }
}
