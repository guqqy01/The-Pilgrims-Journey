using UnityEngine;

namespace GlobalMap.UI
{
    public class GlobalMapArrow : MonoBehaviour
    {
        [SerializeField]
        private RectTransform arrow;

        [SerializeField]
        private float offset = 10f;

        public float Size
        {
            get
            {
                return arrow.localScale.x;
            }
            set
            {
                Vector3 sizeVector = new Vector3(value, value, value);
                arrow.localScale = sizeVector;

                arrow.anchoredPosition = new Vector2(-value * offset, value / 2f);
            }
        }
    }
}

