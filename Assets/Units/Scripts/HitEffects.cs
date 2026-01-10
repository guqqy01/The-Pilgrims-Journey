using UnityEngine;
using System.Collections;
using UI;
namespace Units
{
    public class HitEffects : MonoBehaviour
    {
        [SerializeField]
        private float knockBackSpeed = 1f;

        [SerializeField]
        private float knockBackDuration = 0.2f;

        [SerializeField]
        private Color flashColor = Color.white;

        [SerializeField]
        private float flashDuration = 0.1f;

        private Material unitMat;
        private Color originalColor;

        private Health health;

        private Shield shield;

        private void Awake()
        {
            unitMat = GetComponentInChildren<Renderer>().material;

            if (TryGetComponent(out health))
            {
                health.onHealthChange += HitHealth;
            }

            if(TryGetComponent(out shield))
            {
                shield.onShieldChange += HitShield;
            }
        }

        private void OnDestroy()
        {
            if(shield != null)
            {
                shield.onShieldChange -= HitShield;
            }
            if(health != null)
            {
                health.onHealthChange -= HitHealth;
            }
        }

        public void Init(Material unitMaterial)
        {
            unitMat = unitMaterial;
            originalColor = unitMat.GetColor("_Tint");
        }

        public void HitShield(StatChangeInfo shieldChangeInfo)
        {
            if (shieldChangeInfo.HitInfoType == HitInfoType.Heal)
                return;

            StartCoroutine(FlashMaterial());

            if (shieldChangeInfo.HitInfoType != HitInfoType.Damage || shieldChangeInfo.ChangeAmount <= 0)
                return;

            DamagePopupManager.Instance.SpawnPopup(
                transform.position,
                shieldChangeInfo.ChangeAmount,
                false,
                shieldChangeInfo.HitDirection
            );
            
        }

        private void HitHealth(StatChangeInfo healthChangeInfo)
        {
            if (healthChangeInfo.HitInfoType == HitInfoType.Heal)
                return;

            StartCoroutine(SmoothKnockBack(healthChangeInfo.HitDirection, healthChangeInfo.KnockBackMultiplier));
            StartCoroutine(FlashMaterial());

            if (healthChangeInfo.HitInfoType != HitInfoType.Damage || healthChangeInfo.ChangeAmount <= 0)
                return;

            DamagePopupManager.Instance.SpawnPopup(
                transform.position,
                healthChangeInfo.ChangeAmount,
                true,
                healthChangeInfo.HitDirection
            );
        }

        private IEnumerator SmoothKnockBack(Vector3 direction, float multiplier)
        {
            float totalKnockbackDuration = knockBackDuration;

            float elapsed = 0f;

            while (elapsed < totalKnockbackDuration)
            {
                transform.position += direction.normalized * knockBackSpeed * multiplier * Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }
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