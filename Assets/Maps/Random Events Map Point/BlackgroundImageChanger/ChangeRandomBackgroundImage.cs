using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace RandomEvents
{
    public class ChangeRandomBackgroundImage : Singleton<ChangeRandomBackgroundImage>
    {
        [SerializeField]
        private Animator fadeAnimator;
        [SerializeField]
        private Image backgroundImage;

        private Sprite _sprite;

        public void ChangeBackground(Sprite sprite)
        {
            _sprite = sprite;

            fadeAnimator.SetTrigger("FadeToBlack");
        }

        public void ReadyForImageChange()
        {
            backgroundImage.sprite = _sprite;

            fadeAnimator.SetTrigger("FadeFromBlack");
        }
    }
}

