using UnityEngine;

public abstract class BuffableAttackEntitySO : BuffableEntitySO
{
    [Foldout("Stats")]
    [Header("Attack")]
    [BuffType(true)]
    public Buff DamageBuff;
    [Foldout("Stats")]
    [BuffType(false)]
    public Buff AttackSpeed;
    [Foldout("Stats")]
    [BuffType(false)]
    [Tooltip("critical percentage, value 100 => 100% chance to crit")]
    public Buff CriticalChance;
    [Foldout("Stats")]
    [BuffType(false)]
    [Tooltip("critical damage multiplier, value 100 => +100% damage increase")]
    public Buff CriticalDamageMultiplier;
    [Foldout("Stats")]
    [BuffType(false)]
    public Buff AttackSize;
    [Foldout("Stats")]
    [BuffType(false)]
    public Buff Knockback;
    [Foldout("Stats")]
    [BuffType(true)]
    public Buff NumberOfAttacks;
    [Foldout("Stats")]
    [BuffType(false)]
    public Buff ProjectileSpeed;
    [Foldout("Stats")]
    [BuffType(false)]
    public Buff ProjectileTurnSpeed;
    protected override Buff InheritGetBuff(StatTypes stat)
    {
        return stat switch
        {
            StatTypes.Damage => DamageBuff,
            StatTypes.AttackSpeed => AttackSpeed,
            StatTypes.NumberOfAttacks => NumberOfAttacks,
            StatTypes.CriticalChance => CriticalChance,
            StatTypes.CriticalDamageMultiplier => CriticalDamageMultiplier,
            StatTypes.AttackSize => AttackSize,
            StatTypes.Knockback => Knockback,
            StatTypes.ProjectileSpeed => ProjectileSpeed,
            StatTypes.ProjectileTurnSpeed => ProjectileTurnSpeed,
            _ => null
        };
    }
}

public abstract class BuffableEntitySO : ScriptableObject
{
    [Foldout("Stats")]
    [Header("Defence")]
    [BuffType(true)]
    public Buff Health;
    [BuffType(true)]
    [Foldout("Stats")]
    public Buff Shield;
    [BuffType(false)]
    [Foldout("Stats")]
    public Buff ShieldCooldown;
    [BuffType(false)]
    [Foldout("Stats")]
    public Buff ShieldRegeneration;
    [Header("Utility")]
    [BuffType(false)]
    [Foldout("Stats")]
    public Buff MovementSpeed;

    protected virtual Buff InheritGetBuff(StatTypes stat) { return null; }

    public Buff GetBuff(StatTypes stat)
    {
        return stat switch
        {
            StatTypes.Health => Health,
            StatTypes.Shield => Shield,
            StatTypes.ShieldRegeneration => ShieldRegeneration,
            StatTypes.ShieldCooldown => ShieldCooldown,
            StatTypes.MovementSpeed => MovementSpeed,
            _ => InheritGetBuff(stat)
        };
    }
}
