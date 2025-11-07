using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace RandomEvents
{
    public class ChangeRandomBackgroundImage : MonoBehaviour, ISingletonToBeInitiatedFromRoot
    {
        public static ChangeRandomBackgroundImage Instance { private set; get; }

        public void InitiateSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

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

