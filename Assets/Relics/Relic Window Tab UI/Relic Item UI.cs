using UnityEngine;
using UnityEngine.UI;
namespace Relics
{
    public class RelicItemUI : MonoBehaviour
    {
        private Relic _relic;

        [SerializeField]
        private Image _relicImage;

        public delegate void OnSelect(Relic relic);
        public event OnSelect onSelect;

        public void Setup(Relic relic)
        {
            _relic = relic;
            _relicImage.sprite = _relic.relicSO.sprite;
            _relicImage.NativeRatioScaleToSize(40f);
        }

        public void Select()
        {
            onSelect?.Invoke(_relic);
        }
    }

}
