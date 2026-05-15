using Units;
using UnityEngine;

namespace Tiles
{
    public class TilePlayerPointUI : MonoBehaviour
    {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private RectTransform playerUIPoint;
        [SerializeField]
        private float playerUIPointYOffset;


        [SerializeField]
        private TileGridUI tileGridUI;

        private void Start()
        {
            TilePlayerManager.Instance.OnStopPlaying += OnStopPlaying;
        }

        private void OnDestroy()
        {
            if(TilePlayerManager.Instance != null)
                TilePlayerManager.Instance.OnStopPlaying -= OnStopPlaying;
        }

        public void OnEnable()
        {
            playerUIPoint.position = tileGridUI.GridTransform.position + new Vector3(-tileGridUI.GridTransform.sizeDelta.x, tileGridUI.GridTransform.sizeDelta.y + playerUIPointYOffset) * canvas.scaleFactor / 2f;
        }

        private void Update()
        {
            playerUIPoint.position = tileGridUI.GridTransform.position + new Vector3(-tileGridUI.GridTransform.sizeDelta.x + 2f * TilePlayerManager.Instance.CurrentPlayingPosition * (tileGridUI.GridTransform.sizeDelta.x / tileGridUI.GridSizeX), tileGridUI.GridTransform.sizeDelta.y + playerUIPointYOffset) * canvas.scaleFactor / 2f;

        }

        private void OnStopPlaying()
        {
            playerUIPoint.position = tileGridUI.GridTransform.position + new Vector3(-tileGridUI.GridTransform.sizeDelta.x, tileGridUI.GridTransform.sizeDelta.y + playerUIPointYOffset) * canvas.scaleFactor / 2f;
        }
    }
}


