using UnityEngine;
using UnityEngine.UI;
namespace TPJ.UI
{
    public enum PointUIType
    {
        FullEmpty,
        FullHalf,
        FullFull,
        HalfEmpty,
        HalfFull,
    }

    public class CardPointUI : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private Sprite fullFullPointSprite;
        [SerializeField]
        private Sprite fullHalfPointSprite;
        [SerializeField]
        private Sprite fullEmptyPointSprite;

        [SerializeField]
        private Sprite halfEmptyPointSprite;
        [SerializeField]
        private Sprite halfFullPointSprite;

        public void SetFilled(PointUIType pointUI)
        {
            image.sprite = pointUI switch
            {
                PointUIType.FullEmpty => fullEmptyPointSprite,
                PointUIType.FullHalf => fullHalfPointSprite,
                PointUIType.FullFull => fullFullPointSprite,
                PointUIType.HalfEmpty => halfEmptyPointSprite,
                PointUIType.HalfFull => halfFullPointSprite,
                _ => fullEmptyPointSprite,
            };
        }
    }
}
