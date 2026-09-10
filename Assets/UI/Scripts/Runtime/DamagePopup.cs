using TMPro;
using UnityEditor.Build;
using UnityEngine;

namespace UI
{
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float duration = 1f;
        [SerializeField] private Vector3 floatOffset = new Vector3(0, 1f, 0);

        [SerializeField]
        private Color healthDamageColor = Color.red;
        [SerializeField]
        private Color shieldDamageColor = Color.blue;
        [SerializeField]
        private Color healthCritDamageColor = Color.orangeRed;
        [SerializeField]
        private Color shieldCritDamageColor = Color.cyan;

        [SerializeField]
        [Range(1, 2)]
        private float critSizeMultiplier = 1.5f;
        [SerializeField]
        [Range(0, 2)]
        private float critMovementMultiplier = 1.2f;

        [SerializeField] private AnimationCurve sizeOverLife;
        [SerializeField] private float hitSpeed = 2f;
        [SerializeField] private float fallOfHitSpeed = 2f;

        private Vector3 dmgDirection;
        private float timer;
        private System.Action returnToPool;

        private bool _crit;

        public void Setup(int damage, bool healthGotDmgd, bool crit, Vector3 direction, System.Action returnToPool)
        {
            this.returnToPool = returnToPool;
            _crit = crit;
            float angleVariance = 5f;
            float angleToRotate = Random.Range(-angleVariance, angleVariance);
            dmgDirection = ((Vector2)direction).RotateClockwise(angleToRotate * Mathf.Deg2Rad);

            Color color = Color.white;

            if (healthGotDmgd)
            {
                if (crit)
                {
                    color = healthCritDamageColor;
                }
                else
                {
                    color = healthDamageColor;
                }
            }
            else
            {
                if (crit)
                {
                    color = shieldCritDamageColor;
                }
                else
                {
                    color = shieldDamageColor;
                }
            }

            text.color = color;
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
            movement += (((_crit ? critMovementMultiplier : 1f) * hitSpeed) - fallOfHitSpeed * (duration - timer)) * Time.deltaTime * dmgDirection;

            transform.position += movement;

            float size = (_crit? critSizeMultiplier : 1f) * sizeOverLife.Evaluate(1f - (timer / duration));
            transform.localScale = new Vector3(size, size, 1f);
        }
    }
}

