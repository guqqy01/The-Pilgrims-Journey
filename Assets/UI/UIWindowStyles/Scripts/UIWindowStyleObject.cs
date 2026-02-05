using UnityEngine;
using UnityEngine.UI;

namespace UI.WindowStyles
{
    public class UIWindowStyleObject : MonoBehaviour
    {
        [SerializeField] private Image image;
        public UIWindowStyleLabel label;

        public Image Image
        {
            get
            {
                if (image == null)
                    TryGetComponent(out image);
                return image;
            }
        }

        public void Apply(UIWindowStyle style)
        {
            if (style == null || label == null)
                return;

            if (style.TryGetSprite(label, out var sprite))
                Image.sprite = sprite;
        }
    }

}

