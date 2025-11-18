using UnityEngine;
using System.Collections.Generic;

public class Shield : MonoBehaviour
{
    [SerializeField]
    private int initShield = 5;
    [SerializeField]
    private float initRegen = 1f, initCooldown = 6f;

    private BuffableStat shield = new BuffableStat(0);
    public int MaxShield
    {
        get
        {
            var stat = shield.Value;

            // If this stat is int-based, use IntValue
            if (stat.IsInt)
                return stat.IntValue;

            // If it’s float-based (because of buffs), round it
            return Mathf.RoundToInt(stat.FloatValue);
        }
    }
    public int CurrentShield { private set; get; } = 5;


    private BuffableStat regeneration = new BuffableStat(1f);
    public float Regeneration => (float)regeneration.Value.Value;


    private BuffableStat cooldown = new BuffableStat(6f);
    public float Cooldown => (float)cooldown.Value.Value;

    private float timeToNextRegenPoint;

    private readonly Dictionary<string, float> referenceAndTimeToRemoveBuff = new Dictionary<string, float>();

    public delegate void OnDestroy();
    public OnDestroy onDestroy;

    public delegate void OnShieldChange(StatChangeInfo statChangeInfo);
    public OnShieldChange onShieldChange;

    [SerializeField]
    private ParticleSystem shieldBrokenPS;

    private void Start()
    {
        Init(initShield, initRegen, initCooldown);
    }

    public void Init(int maxShield, float regeneration, float shieldCooldown)
    {
        shield = new BuffableStat(maxShield);

        CurrentShield = MaxShield;

        regeneration = new BuffableStat(regeneration);

        cooldown = new BuffableStat(shieldCooldown);

        StatChangeInfo statChangeInfo = new StatChangeInfo(HitStatType.Shield, HitInfoType.Init, CurrentShield, CurrentShield);

        onShieldChange?.Invoke(statChangeInfo);
    }

    public void Init(BuffableStat shieldStat, BuffableStat shieldRegenerationStat, BuffableStat shieldCooldownStat)
    {
        shield = shieldStat;

        CurrentShield = MaxShield;

        regeneration = shieldRegenerationStat;

        cooldown = shieldCooldownStat;

        StatChangeInfo statChangeInfo = new StatChangeInfo(HitStatType.Shield, HitInfoType.Init, CurrentShield, CurrentShield);

        onShieldChange?.Invoke(statChangeInfo);
    }

    private void Update()
    {
        HandleRegeneration();
        HandleAdditionalTimedShield();
    }

    private void HandleAdditionalTimedShield()
    {
        if (referenceAndTimeToRemoveBuff.Count <= 0)
            return;

        List<string> toRemove = new List<string>();

        int amountChanged = 0;

        foreach(KeyValuePair<string, float> kvp in referenceAndTimeToRemoveBuff)
        {
            if(Time.time >= kvp.Value)
            {
                amountChanged += shield.RemoveBuff(kvp.Key);
                    
                toRemove.Add(kvp.Key);
            }
        }

        CurrentShield = Mathf.Min(CurrentShield, MaxShield);

        StatChangeInfo statChangeInfo = new StatChangeInfo(HitStatType.Shield, amountChanged, CurrentShield);

        onShieldChange?.Invoke(statChangeInfo);

        if (toRemove.Count <= 0)
            return;

        for (int i = 0; i < toRemove.Count; i++)
        {
            referenceAndTimeToRemoveBuff.Remove(toRemove[i]);
        }
    }

    private void HandleRegeneration()
    {
        if (shield == null || shield.Value == null)
            return;

        if (CurrentShield == MaxShield) return;

        if (Time.time >= timeToNextRegenPoint)
        {
            RegenerateShield(1);
            timeToNextRegenPoint = Time.time + 1f / Regeneration;
        }
    }

    public void RegenerateShield(int amount)
    {
        CurrentShield = Mathf.Clamp(CurrentShield + amount, 0, MaxShield);

        StatChangeInfo statChangeInfo = new StatChangeInfo(HitStatType.Shield, amount, CurrentShield);

        onShieldChange?.Invoke(statChangeInfo);
    }
    
    /// <summary>
    /// Deal damage to the shield
    /// </summary>
    /// <param name="amount">The amount of dmg dealt to the shield</param>
    /// <returns>Returns the amount of damage left over</returns>
    public int DamageShield(int amount, Vector2 hitDirection)
    {
        int leftOverDmg = Mathf.Max(amount - CurrentShield, 0);

        CurrentShield = Mathf.Clamp(CurrentShield - amount, 0, MaxShield);

        StatChangeInfo statChangeInfo = new StatChangeInfo(HitStatType.Shield, -amount + leftOverDmg, CurrentShield, hitDirection);

        onShieldChange?.Invoke(statChangeInfo);

        if (amount > 0) timeToNextRegenPoint = Regeneration != 0 ? Time.time + Cooldown : float.PositiveInfinity;

        if (CurrentShield == 0 && amount != leftOverDmg)
        {
            onDestroy?.Invoke();
            if(shieldBrokenPS != null)
                shieldBrokenPS?.Play();
        }

        return leftOverDmg;
    }

    

    public void ModifyShieldForTime(string reference, Buff shieldBuff, float time)
    {
        RegenerateShield((int)shield.AddBuff(reference, shieldBuff));

        if (referenceAndTimeToRemoveBuff.ContainsKey(reference))
        {
            referenceAndTimeToRemoveBuff[reference] = Time.time + time;
        }
        else
        {
            referenceAndTimeToRemoveBuff.Add(reference, Time.time + time);
        }
    }

    public void ModifyShield(string reference, Buff shieldBuff)
    {
        RegenerateShield((int)shield.AddBuff(reference, shieldBuff));
    }
}


