using UnityEngine;

[RequireComponent(typeof(Health), typeof(Shield))]
public class HealthAndShieldManager : MonoBehaviour
{
    public Health Health { private set; get; }
    public Shield Shield { private set; get; }
    private Material UnitMat;

    public bool Invunrable { get; private set; }
    protected float removeInvunrabilityTime;
    private const float percentageOfRemainingTimeToStartFlickering = 0.3f;
    private const float percentageOfRemainingTimeToChangeFlickerState = 0.2f;
    private float timeToStartFlickering;
    private float timeToChangeFlickerColor;
    private bool flickerOnOrOff;
    private Color invunrabilityColor = Color.blue;


    private void Awake()
    {
        Health = GetComponent<Health>();
        Shield = GetComponent<Shield>();
        UnitMat = GetComponentInChildren<Renderer>().material;
    }

    public void Setup(BuffableStat health, BuffableStat shield, BuffableStat shieldRegeneration, BuffableStat shieldCooldown)
    {
        Health.Init(health);

        Shield.Init(shield, shieldRegeneration, shieldCooldown);
    }

    private void Update()
    {
        if (Invunrable && Time.time >= removeInvunrabilityTime)
        {
            RemoveShock();
        }
        else if (Invunrable && Time.time >= removeInvunrabilityTime - timeToStartFlickering)
        {
            Flicker();
        }
    }

    private void Flicker()
    {
        if (Time.time >= timeToChangeFlickerColor)
        {
            UnitMat.SetColor("_Tint", flickerOnOrOff ? invunrabilityColor : Color.white);
            timeToChangeFlickerColor = Time.time + (removeInvunrabilityTime - Time.time) * percentageOfRemainingTimeToChangeFlickerState;
            flickerOnOrOff = !flickerOnOrOff;
        }
    }

    public void MakeInvunrableForTime(float shockTime)
    {
        Invunrable = true;

        timeToStartFlickering = shockTime * percentageOfRemainingTimeToStartFlickering;

        removeInvunrabilityTime = Time.time + shockTime;

        UnitMat.SetFloat("_EffectAmount", 1);

        UnitMat.SetColor("_Tint", invunrabilityColor);
    }

    private void RemoveShock()
    {
        Invunrable = false;

        UnitMat.SetFloat("_EffectAmount", 0);
    }

    public OnHitInfo Damage(int damage, Vector2 damageDirection, float knockBackMultiplier = 1f)
    {
        if (Invunrable)
            return new OnHitInfo(this, false, 0, 0);

        int leftDmg = Shield.DamageShield(damage, damageDirection);

        OnHitInfo hitInfo = new OnHitInfo(this, leftDmg >= Health.CurrentHealth, damage - leftDmg, Mathf.Min(Health.CurrentHealth, leftDmg));

        if (leftDmg > 0)
        {
            Health.ModifyHealth(-leftDmg, damageDirection, knockBackMultiplier);
        }

        return hitInfo;
    }

    public OnHitInfo Damage(AttackStats hitStats, Vector2 damageDirection)
    {
        if (Invunrable)
            return new OnHitInfo(this, false, 0, 0);

        BuffableStat damage = new BuffableStat(hitStats.damage);

        if(Random.value <= hitStats.criticalChance / 100f)
        {
            damage.AddBuff("Critical", new Buff(hitStats.criticalDamageMultiplier, 0, 0));
        }

        int leftDmg = Shield.DamageShield(damage, damageDirection);

        OnHitInfo hitInfo = new OnHitInfo(this, leftDmg >= Health.CurrentHealth, damage - leftDmg, Mathf.Min(Health.CurrentHealth, leftDmg));

        if (leftDmg > 0)
        {
            Health.ModifyHealth(-leftDmg, damageDirection, hitStats.knockbackMultiplier);
        }

        return hitInfo;
    }
}

