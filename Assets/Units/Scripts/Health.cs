using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int initMaxHealth = 10;

    private BuffableStat maxHealth;
    public int MaxHealth
    {
        get
        {
            return maxHealth.Value.IntValue;
        }
    }

    public int CurrentHealth { private set; get; } = 5;

    public delegate void OnDie(Health health);
    public OnDie onDie;

    public delegate void OnHealthChange(StatChangeInfo statChangeInfo);
    public OnHealthChange onHealthChange;

    public void Init(BuffableStat healthBuffableStat)
    {
        maxHealth = healthBuffableStat;

        CurrentHealth = MaxHealth;

        StatChangeInfo statChangeInfo = new StatChangeInfo(HitStatType.Health, HitInfoType.Init, CurrentHealth, CurrentHealth);

        onHealthChange?.Invoke(statChangeInfo);
    }

    public void Init(int maxHealth)
    {
        maxHealth = new BuffableStat(maxHealth);

        CurrentHealth = MaxHealth;

        StatChangeInfo statChangeInfo = new StatChangeInfo(HitStatType.Health, HitInfoType.Init, CurrentHealth, CurrentHealth);

        onHealthChange?.Invoke(statChangeInfo);
    }

    public void ModifyHealth(int amount, Vector2 sourceDirection = new Vector2(), float knockBackMultiplier = 0)
    {
        CurrentHealth = Mathf.Clamp(amount + CurrentHealth, 0, MaxHealth);

        StatChangeInfo statChangeInfo = new StatChangeInfo(HitStatType.Health, amount, CurrentHealth, sourceDirection, knockBackMultiplier);

        onHealthChange?.Invoke(statChangeInfo);

        if (CurrentHealth <= 0)
        {
            onDie?.Invoke(this);

            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        onDie = null;
        onHealthChange = null;
    }
}

public enum HitInfoType
{
    Init,
    Damage,
    Heal,
}

public enum HitStatType
{
    Health,
    Shield,
}

public class StatChangeInfo
{
    public readonly int CurrentAmount;
    public readonly int ChangeAmount;
    public readonly Vector2 HitDirection;
    public readonly float KnockBackMultiplier;
    public readonly HitStatType Stat;
    public readonly HitInfoType HitInfoType;

    public void Debug()
    {
        UnityEngine.Debug.Log($"Current Amount : {CurrentAmount}\r\n" +
            $"Change Amount : {ChangeAmount}\r\n" +
            $"Hit Direction: {HitDirection}\r\n" +
            $"Knock Back Multiplier: {KnockBackMultiplier}\r\n" +
            $"Stat : {Stat}\r\n" +
            $"InfoType : {HitInfoType}");
    }

    public StatChangeInfo(HitStatType hitStatType, int modifyAmount, int currentAmount, Vector2 hitDirection = new Vector2(), float knockBackMultiplier = 0)
    {
        Stat = hitStatType;
        CurrentAmount = currentAmount;
        if (modifyAmount > 0)
        {
            HitInfoType = HitInfoType.Heal;
        }
        else
        {
            HitInfoType = HitInfoType.Damage;
        }

        ChangeAmount = Mathf.Abs(modifyAmount);
        HitDirection = hitDirection;
        KnockBackMultiplier = knockBackMultiplier;
    }

    public StatChangeInfo(HitStatType hitStatType, HitInfoType hitInfoType,int modifyAmount, int currentAmount, Vector2 hitDirection = new Vector2(), float knockBackMultiplier = 0)
    {
        Stat = hitStatType;
        HitInfoType = hitInfoType;
        CurrentAmount = currentAmount;
        ChangeAmount = Mathf.Abs(modifyAmount);
        HitDirection = hitDirection;
        KnockBackMultiplier = knockBackMultiplier;
    }
}

