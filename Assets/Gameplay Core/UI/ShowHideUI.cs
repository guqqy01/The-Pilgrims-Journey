using System.Collections;
using UnityEngine;

namespace Units.UI
{
    public class ShowHideUI : MonoBehaviour
    {
        [SerializeField]
        private float hideUITimeLength = 4f;

        [SerializeField]
        private float showHideUISpeed = 0.2f;
        [SerializeField]
        private CanvasGroup canvasGroup;

        private float _timeToHideUI;

        private Coroutine _showHideCoroutine;

        private bool _hiding = false;

        private void Update()
        {
            if (canvasGroup == null)
                return;

            if (!_hiding && canvasGroup.alpha != 0 && Time.time >= _timeToHideUI)
            {
                HideUI();
            }
        }

        protected void ShowUI()
        {
            if (canvasGroup == null)
                return;

            if (_showHideCoroutine != null)
                StopCoroutine(_showHideCoroutine);

            
            _showHideCoroutine = StartCoroutine(ShowHideUICoroutine(true));
            _hiding = false;
            _timeToHideUI = Time.time + hideUITimeLength;
        }

        private void HideUI()
        {
            if (canvasGroup == null)
                return;

            if (_showHideCoroutine != null)
                StopCoroutine(_showHideCoroutine);
            _hiding = true;
            _showHideCoroutine = StartCoroutine(ShowHideUICoroutine(false));
        }

        private IEnumerator ShowHideUICoroutine(bool show)
        {
            if (!show)
                _hiding = true;

            float startAlpha = canvasGroup.alpha;
            float endAlphaTarget = show ? 1f : 0f;

            // How far along we already are toward the target (0 = not started, 1 = already there)
            float alphaT = Mathf.InverseLerp(show ? 0f : 1f, show ? 1f : 0f, startAlpha);
            // Remaining time shrinks as we approach the target
            float duration = Mathf.Lerp(showHideUISpeed, 0f, alphaT);

            float startTime = Time.time;
            float endTime = startTime + duration;

            while (Time.time < endTime)
            {
                float t = duration > 0f ? (Time.time - startTime) / duration : 1f;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlphaTarget, t);
                yield return null;
            }

            _hiding = false;
            canvasGroup.alpha = endAlphaTarget;
            _showHideCoroutine = null;
        }
    }
}

