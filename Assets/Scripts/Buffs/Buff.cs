using System;
using UnityEngine;
using static UnityEngine.UI.Image;

[Serializable]
public class Buff
{
    [SerializeField]
    private UnitStatsEnum stat;
    public UnitStatsEnum Stat => stat;
    /// <summary>
    /// Multiplies the base buff: BaseBuff * MultiplierPercentage + AdditionalBuff, in percentages, e.g a value of 100 multiplies the base value by +100%
    /// </summary>
    public float Multiplier = 0f;

    /// <summary>
    /// Is the base buffs, get multiplied by the Multiplier: BaseBuff * Multiplier + AdditionalBuff
    /// </summary>
    public IntOrFloatObject BaseBuff;

    /// <summary>
    /// Additional buff is unaffected by multiplier: BaseBuff * Multiplier + AdditionalBuff
    /// </summary>
    public IntOrFloatObject AdditionalBuff;

    public int GetBuffGoldValue()
    {
        if(Stat == UnitStatsEnum.Health || Stat == UnitStatsEnum.Shield)
        {
            if (!BaseBuff.IsInt)
                BaseBuff.IsInt = true;

            if (!AdditionalBuff.IsInt)
                AdditionalBuff.IsInt = true;
        }
        else
        {
            if (BaseBuff.IsInt)
                BaseBuff.IsInt = false;

            if (AdditionalBuff.IsInt)
                AdditionalBuff.IsInt = false;
        }

        return BuffValueCalculator.GetValue(this, out int baseBuffValue, out int multiplierBuffValue, out int additionalBuffValue);
    }

    public static Buff operator +(Buff buff1, Buff buff2)
    {
        if (buff1.Stat != buff2.Stat)
            throw new ArgumentException("Can only add same buffs, Buff1: " + buff1.Stat + ", Buff2: " + buff2.Stat);

        return new Buff(buff1.Stat, buff1.Multiplier + buff2.Multiplier, buff1.BaseBuff + buff2.BaseBuff, buff1.AdditionalBuff + buff2.AdditionalBuff);
    }

    public Buff(UnitStatsEnum stat)
    {
        this.stat = stat;
        Multiplier = 0f;

        if (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield)
        {
            BaseBuff = new IntOrFloatObject(0);
            AdditionalBuff = new IntOrFloatObject(0);
        }
        else
        {
            BaseBuff = new IntOrFloatObject(0f);
            AdditionalBuff = new IntOrFloatObject(0f);
        }
    }

    public Buff(UnitStatsEnum stat, float multiplier, IntOrFloatObject baseBuff, IntOrFloatObject additionalBuff)
    {
        this.stat = stat;
        Multiplier = multiplier;
        BaseBuff = baseBuff;
        AdditionalBuff = additionalBuff;
    }

    public Buff(UnitStatsEnum stat, float multiplier, object baseBuff, object additionalBuff)
    {
        this.stat = stat;
        Multiplier = multiplier;

        if(stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield)
        {
            BaseBuff = new IntOrFloatObject((int)baseBuff);
            AdditionalBuff = new IntOrFloatObject((int)additionalBuff);
        }
        else
        {
            BaseBuff = new IntOrFloatObject((float)baseBuff);
            AdditionalBuff = new IntOrFloatObject((float)additionalBuff);
        }
    }


    public Buff(UnitStatsEnum stat, Buff buff)
    {
        this.stat = stat;
        this.Multiplier = buff.Multiplier;
        this.BaseBuff = buff.BaseBuff;
        this.AdditionalBuff = buff.AdditionalBuff;
    }


    public virtual IntOrFloatObject Value(IntOrFloatObject baseValue)
    {
        return ((baseValue + BaseBuff) * (1 + (Multiplier / 100f))) + AdditionalBuff;
    }
}


public static class BuffValueCalculator
{
    public static int roundToNearestValue = 10;

    public static float additionalReducedValueToBase = 0.8f;

    public static int GetValue(Buff buff, out int baseBuffValue, out int multiplierBuffValue, out int additionalBuffValue)
    {
        switch (buff.Stat)
        {
            case UnitStatsEnum.Health:
                baseBuffValue = CalculateHealthBaseBuffValue((int)buff.BaseBuff.Value);
                multiplierBuffValue = CalculateHealthMultiplierBuffValue(buff.Multiplier);
                additionalBuffValue = CalculateHealthAdditionalBuffValue((int)buff.AdditionalBuff.Value);
                return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
            case UnitStatsEnum.Shield:
                baseBuffValue = CalculateShieldBaseBuffValue((int)buff.BaseBuff.Value);
                multiplierBuffValue = CalculateShieldMultiplierBuffValue(buff.Multiplier);
                additionalBuffValue = CalculateShieldAdditionalBuffValue((int)buff.AdditionalBuff.Value);
                return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
            case UnitStatsEnum.ShieldCooldown:
                baseBuffValue = CalculateShieldCooldownBaseBuffValue((float)buff.BaseBuff.Value);
                multiplierBuffValue = CalculateShieldCooldownMultiplierBuffValue(buff.Multiplier);
                additionalBuffValue = CalculateShieldCooldownAdditionalBuffValue((float)buff.AdditionalBuff.Value);
                return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
            case UnitStatsEnum.ShieldRegeneration:
                baseBuffValue = CalculateShieldRegenerationBaseBuffValue((float)buff.BaseBuff.Value);
                multiplierBuffValue = CalculateShieldRegenerationMultiplierBuffValue(buff.Multiplier);
                additionalBuffValue = CalculateShieldRegenerationAdditionalBuffValue((float)buff.AdditionalBuff.Value);
                return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
            case UnitStatsEnum.MovementSpeed:
                baseBuffValue = CalculateMovementSpeedBaseBuffValue((float)buff.BaseBuff.Value);
                multiplierBuffValue = CalculateMovementSpeedMultiplierBuffValue(buff.Multiplier);
                additionalBuffValue = CalculateMovementSpeedAdditionalBuffValue((float)buff.AdditionalBuff.Value);
                return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
            case UnitStatsEnum.SpawnRate:
                baseBuffValue = CalculateSpawnRateBaseBuffValue((float)buff.BaseBuff.Value);
                multiplierBuffValue = CalculateSpawnRateMultiplierBuffValue(buff.Multiplier);
                additionalBuffValue = CalculateSpawnRateAdditionalBuffValue((float)buff.AdditionalBuff.Value);
                return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
        }

        baseBuffValue = 0;
        multiplierBuffValue = 0;
        additionalBuffValue = 0;

        return 0;
    }


    #region Funcitons

    public static int Linear(float value, float multiplierPerValue)
    {
        return Mathf.RoundToInt(value * multiplierPerValue);
    }

    public static int LinearCenterAt1(float value, float multiplierPerValue)
    {
        return Mathf.RoundToInt((value - 1) * multiplierPerValue);
    }

    public static int CubedCenteredAt1(float value, float multiplier)
    {
        return Mathf.RoundToInt(multiplier * (Mathf.Pow(value - 1, 3)));
    }

    public static int InvertedCubedCenteredAt1(float value, float multiplier)
    {
        return Mathf.RoundToInt(-multiplier * (Mathf.Pow(value - 1, 3)));
    }

    public static int Quadratic(float value, float multiplierA, float xAxisIntersection)
    {
        return Mathf.RoundToInt(multiplierA * (xAxisIntersection - Mathf.Pow(value, 2)));
    }

    #endregion

    #region Health

    private const int valuePerBaseHealthPoint = 10;

    private const float healthMultiplierA = 10f;

    public static int CalculateHealthBaseBuffValue(int baseValue)
    {
        return baseValue * valuePerBaseHealthPoint;
    }

    public static int CalculateHealthMultiplierBuffValue(float multiplier)
    {
        return Linear(multiplier, healthMultiplierA);
    }

    public static int CalculateHealthAdditionalBuffValue(int additionalValue)
    {
        return Mathf.RoundToInt(additionalValue * valuePerBaseHealthPoint * additionalReducedValueToBase);
    }

    #endregion

    #region Shield

    private const int valuePerBaseShieldPoint = 10;

    private const float shieldMultiplierA = 10f;

    public static int CalculateShieldBaseBuffValue(int baseValue)
    {
        return baseValue * valuePerBaseShieldPoint;
    }

    public static int CalculateShieldMultiplierBuffValue(float multiplier)
    {
        return Linear(multiplier, shieldMultiplierA);
    }

    public static int CalculateShieldAdditionalBuffValue(int additionalValue)
    {
        return Mathf.RoundToInt(additionalValue * valuePerBaseShieldPoint * additionalReducedValueToBase);
    }

    #endregion

    #region ShieldCooldown

    private const int valuePerBaseShieldCooldownPoint = -200;

    private const float shieldCooldownMultiplierA = 10f;

    public static int CalculateShieldCooldownBaseBuffValue(float baseValue)
    {
        return Mathf.RoundToInt(baseValue * valuePerBaseShieldCooldownPoint);
    }

    public static int CalculateShieldCooldownMultiplierBuffValue(float multiplier)
    {
        return Linear(-multiplier, shieldCooldownMultiplierA);
    }

    public static int CalculateShieldCooldownAdditionalBuffValue(float additionalValue)
    {
        return Mathf.RoundToInt(additionalValue * valuePerBaseShieldCooldownPoint * additionalReducedValueToBase);
    }

    #endregion

    #region ShieldRegeneration

    private const int valuePerBaseShieldRegenerationPoint = 100;

    private const float shieldRegenerationMultiplierA = 10f;

    public static int CalculateShieldRegenerationBaseBuffValue(float baseValue)
    {
        return Mathf.RoundToInt(baseValue * valuePerBaseShieldRegenerationPoint);
    }

    public static int CalculateShieldRegenerationMultiplierBuffValue(float multiplier)
    {
        return Linear(multiplier, shieldRegenerationMultiplierA);
    }

    public static int CalculateShieldRegenerationAdditionalBuffValue(float additionalValue)
    {
        return Mathf.RoundToInt(additionalValue * valuePerBaseShieldRegenerationPoint * additionalReducedValueToBase);
    }

    #endregion

    #region MovementSpeed

    private const int valuePerBaseMovementSpeedPoint = 500;

    private const float movementSpeedMultiplierA = 10f;

    public static int CalculateMovementSpeedBaseBuffValue(float baseValue)
    {
        return Mathf.RoundToInt(baseValue * valuePerBaseMovementSpeedPoint);
    }

    public static int CalculateMovementSpeedMultiplierBuffValue(float multiplier)
    {
        return Linear(multiplier, movementSpeedMultiplierA);
    }

    public static int CalculateMovementSpeedAdditionalBuffValue(float additionalValue)
    {
        return Mathf.RoundToInt(additionalValue * valuePerBaseMovementSpeedPoint * additionalReducedValueToBase);
    }

    #endregion

    #region SpawnRate

    private const int valuePerBaseSpawnRatePoint = 500;

    private const float spawnRateMultiplierA = 10f;

    public static int CalculateSpawnRateBaseBuffValue(float baseValue)
    {
        return Mathf.RoundToInt(baseValue * valuePerBaseSpawnRatePoint);
    }

    public static int CalculateSpawnRateMultiplierBuffValue(float multiplier)
    {
        return Linear(multiplier, spawnRateMultiplierA);
    }

    public static int CalculateSpawnRateAdditionalBuffValue(float additionalValue)
    {
        return Mathf.RoundToInt(additionalValue * valuePerBaseSpawnRatePoint * additionalReducedValueToBase);
    }

    #endregion
}