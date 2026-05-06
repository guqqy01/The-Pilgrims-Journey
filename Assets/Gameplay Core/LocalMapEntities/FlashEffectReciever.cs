using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DamageRecieverEffectManager))]
[DisallowMultipleComponent]
public class FlashEffectReciever : DamageRecieverEffect
{
    [SerializeField]
    private Color flashColor = Color.white;

    [SerializeField]
    private float flashDuration = 0.1f;

    private Material unitMat;
    private Color originalColor;

    private void Awake()
    {
        unitMat = GetComponentInChildren<Renderer>().material;
        originalColor = unitMat.GetColor("_Tint");
    }

    public override void OnHitShield(StatChangeInfo shieldChangeInfo)
    {
        if (shieldChangeInfo == null)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        if (shieldChangeInfo.HitInfoType == HitInfoType.Heal)
            return;

        StartCoroutine(FlashMaterial());
    }

    public override void OnHitHealth(StatChangeInfo healthChangeInfo)
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
