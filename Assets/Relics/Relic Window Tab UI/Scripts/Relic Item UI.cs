using UnityEngine;
using UnityEngine.UI;
namespace Relics
{
    public class RelicItemUI : MonoBehaviour
    {
        public Relic Relic { private set; get; }

        [SerializeField]
        private Image _relicImage;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Sprite selectedBackgroundSprite;
        [SerializeField]
        private Sprite unselectedBackgroundSprite;

        public delegate void OnSelect(Relic relic);
        public event OnSelect onSelect;

        public void Setup(Relic relic)
        {
            Relic = relic;
            _relicImage.sprite = Relic.relicSO.sprite;
            _relicImage.NativeRatioScaleToSize(40f);
        }

        public void Select()
        {
            backgroundImage.sprite = selectedBackgroundSprite;
            onSelect?.Invoke(Relic);
        }

        public void Unselect()
        {
            backgroundImage.sprite = unselectedBackgroundSprite;
        }
    }

}
