using UnityEngine;
using System.Collections;

public class KnockbackEffectReciever : MonoBehaviour
{
    private Health health;

    private void Awake()
    {
        if (TryGetComponent(out health))
        {
            health.onHealthChange += Knockback;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.onHealthChange -= Knockback;
        }
    }

    [SerializeField]
    private float knockBackSpeed = 1f;

    [SerializeField]
    private float knockBackDuration = 0.2f;

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

    public void Knockback(StatChangeInfo healthChangeInfo)
    {
        if (healthChangeInfo == null)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        if (healthChangeInfo.HitInfoType == HitInfoType.Heal)
            return;

        StartCoroutine(SmoothKnockBack(healthChangeInfo.HitDirection, healthChangeInfo.KnockBackMultiplier));
    }
}
