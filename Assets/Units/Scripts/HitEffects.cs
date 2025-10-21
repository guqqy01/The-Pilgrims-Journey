using UnityEngine;
using System.Collections;

namespace Units
{
    public class HitEffects : MonoBehaviour
    {
        [SerializeField]
        private float knockBack = 5f;

        [SerializeField]
        private float knockBackDuration = 0.2f;

        [SerializeField]
        private Color flashColor = Color.white;

        [SerializeField]
        private float flashDuration = 0.1f;

        private Material unitMat;
        private Color originalColor;

        [SerializeField]
        private DamagePopup damagePopupPrefab;

        
        public void Init(Material unitMaterial)
        {
            unitMat = unitMaterial;
            originalColor = unitMat.GetColor("_Tint");
        }

        public void HitShield(int dmgAmount, Vector3 hitDirection)
        {
            StartCoroutine(FlashMaterial());

            if(dmgAmount > 0)
            {
                DamagePopup damagePopup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
                damagePopup.Setup(dmgAmount, false, hitDirection);
            }
        }

        public void HitHealth(int dmgAmount, float knockBackMultiplier, Vector3 hitDirection)
        {
            StartCoroutine(SmoothKnockBack(hitDirection, knockBackMultiplier));
            StartCoroutine(FlashMaterial());

            if (dmgAmount > 0)
            {
                DamagePopup damagePopup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
                damagePopup.Setup(dmgAmount, true, hitDirection);
            }
        }

        private IEnumerator SmoothKnockBack(Vector3 direction, float multiplier)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + direction.normalized * knockBack * multiplier;
            float elapsed = 0f;

            while (elapsed < knockBackDuration)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / knockBackDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
        }

        private IEnumerator FlashMaterial()
        {
            if (unitMat != null)
            {
                unitMat.SetInt("_OneColorEffect", 1);
                unitMat.SetColor("_OneColor", flashColor);
                yield return new WaitForSeconds(flashDuration);
                unitMat.SetFloat("_OneColorEffect", 0);
                unitMat.SetColor("_OneColor", originalColor);
            }
        }
    }
}