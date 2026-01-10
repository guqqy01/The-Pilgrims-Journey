using TMPro;
using UnityEngine;

namespace UI
{
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float duration = 1f;
        [SerializeField] private Vector3 floatOffset = new Vector3(0, 1f, 0);
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite redHeartSprite;
        [SerializeField] private Sprite blueHeartSprite;
        [SerializeField] private AnimationCurve sizeOverLife;
        [SerializeField] private float hitSpeed = 2f;
        [SerializeField] private float fallOfHitSpeed = 2f;

        private Vector3 dmgDirection;
        private float timer;
        private System.Action returnToPool;

        public void Setup(int damage, bool healthGotDmgd, Vector3 direction, System.Action returnToPool)
        {
            this.returnToPool = returnToPool;

            float angleVariance = 5f;
            float angleToRotate = Random.Range(-angleVariance, angleVariance);
            dmgDirection = ((Vector2)direction).RotateClockwise(angleToRotate * Mathf.Deg2Rad);

            spriteRenderer.sprite = healthGotDmgd ? redHeartSprite : blueHeartSprite;
            text.text = damage.ToString();
            timer = duration;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                returnToPool?.Invoke();
                return;
            }

            Vector3 movement = floatSpeed * Time.deltaTime * floatOffset;
            movement += (hitSpeed - fallOfHitSpeed * (duration - timer)) * Time.deltaTime * dmgDirection;

            transform.position += movement;

            float size = sizeOverLife.Evaluate(1f - (timer / duration));
            transform.localScale = new Vector3(size, size, 1f);
        }
    }
}

