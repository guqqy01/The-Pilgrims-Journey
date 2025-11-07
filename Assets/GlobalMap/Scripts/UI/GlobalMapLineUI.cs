using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GlobalMap.UI
{
    public class GlobalMapLineUI : MonoBehaviour
    {
        [SerializeField]
        private Image mainLine;
        [SerializeField]
        private Image secondaryLine;
        [SerializeField]
        private RectTransform rectTransform;
        [SerializeField]
        private float timeitTakesToChangeColor = 1;

        [SerializeField]
        public Color newColor;
        [SerializeField]
        public float _t;

        Coroutine colorChangeCoroutine;
        public Color Color
        {
            get
            {
                return mainLine.color;
            }
            set
            {
                if (colorChangeCoroutine != null)
                    return;

                mainLine.color = value;
            }
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Setup(Vector2 fromPos, Vector2 toPos, float lineWidth, float canvasScale)
        {
            rectTransform.position = fromPos;
            float y = toPos.y - fromPos.y;
            float x = toPos.x - fromPos.x;
            Vector3 eulerRotation = new Vector3(0, 0, Mathf.Atan2(y, x) * Mathf.Rad2Deg);
            rectTransform.rotation = Quaternion.Euler(eulerRotation);
            rectTransform.sizeDelta = new Vector2(Vector2.Distance(fromPos, toPos) / canvasScale, lineWidth);
        }



        public void PlayColorChange(Color newColor, Color oldColor)
        {
            if (colorChangeCoroutine != null)
                StopCoroutine(colorChangeCoroutine);


            colorChangeCoroutine = StartCoroutine(ChangeColorMovingLeftToRight(newColor, oldColor));
        }

        private IEnumerator ChangeColor(Color newColor)
        {
            yield return ChangeColorMovingLeftToRight(newColor, Color);
        }

        private IEnumerator ChangeColorMovingLeftToRight(Color leftColor, Color rightColor)
        {
            float t = 0;

            float startTime = Time.time;

            while (t < 1)
            {
                SetColorGradient(leftColor, rightColor, t);
                t = Mathf.InverseLerp(startTime, startTime + timeitTakesToChangeColor, Time.time);
                yield return null;
            }
            SetColorGradient(leftColor, rightColor, 1);
            secondaryLine.gameObject.SetActive(false);
            Color = leftColor;
            colorChangeCoroutine = null;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"> if t = 0 only right color shown, if t == 1 only left color shown </param>
        public void SetColorGradient(Color leftColor, Color rightColor, float t)
        {
            mainLine.color = leftColor;
            if (!secondaryLine.isActiveAndEnabled) secondaryLine.gameObject.SetActive(true);
            secondaryLine.color = rightColor;

            float width = rectTransform.sizeDelta.x;

            mainLine.rectTransform.offsetMax = new Vector2(-Mathf.Lerp(width, 0, t), 0);
            secondaryLine.rectTransform.offsetMin = new Vector2(Mathf.Lerp(0, width, t), 0);
        }
    }
}
