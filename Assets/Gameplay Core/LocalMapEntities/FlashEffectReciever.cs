using UnityEngine;
using System.Collections;

public class FlashEffectReciever : MonoBehaviour
{
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

        if (TryGetComponent(out shield))
        {
            shield.onShieldChange += HitShield;
        }
    }

    private void OnDestroy()
    {
        if (shield != null)
        {
            shield.onShieldChange -= HitShield;
        }
        if (health != null)
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
        if (shieldChangeInfo == null)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        if (shieldChangeInfo.HitInfoType == HitInfoType.Heal)
            return;

        StartCoroutine(FlashMaterial());
    }

    private void HitHealth(StatChangeInfo healthChangeInfo)
    {
        if (healthChangeInfo == null)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        if (healthChangeInfo.HitInfoType == HitInfoType.Heal)
            return;

        StartCoroutine(FlashMaterial());
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
