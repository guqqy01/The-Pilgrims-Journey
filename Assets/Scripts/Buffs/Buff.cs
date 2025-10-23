using System;
using UnityEngine;

[Serializable]
public class Buff
{
    [Tooltip("Percentage multiplier. Example: 100 = +100% (doubles the base).")]
    public float Multiplier = 0f;

    [SerializeField] private IntOrFloatObject baseBuff;
    [SerializeField] private IntOrFloatObject additionalBuff;

    public IntOrFloatObject BaseBuff => baseBuff;
    public IntOrFloatObject AdditionalBuff => additionalBuff;

    public bool IsInt
    {
        get
        {
            return baseBuff.IsInt;
        }
    }
    public Buff(bool isInt)
    {
        Multiplier = 0f;

        if (isInt)
        {
            baseBuff = new IntOrFloatObject(0);
            additionalBuff = new IntOrFloatObject(0);
        }
        else
        {
            baseBuff = new IntOrFloatObject(0f);
            additionalBuff = new IntOrFloatObject(0f);
        }
    }

    public Buff(float multiplier, IntOrFloatObject baseBuff, IntOrFloatObject additionalBuff)
    {
        Multiplier = multiplier;
        this.baseBuff = baseBuff;
        this.additionalBuff = additionalBuff;
    }

    /// <summary>
    /// (baseValue + BaseBuff) * (1 + Multiplier/100) + AdditionalBuff
    /// </summary>
    public virtual IntOrFloatObject Value(IntOrFloatObject baseValue)
    {
        float result = ((baseValue + baseBuff) * (1 + Multiplier / 100f) + additionalBuff).FloatValue;

        return baseBuff.IsInt
            ? new IntOrFloatObject(Mathf.RoundToInt(result))
            : new IntOrFloatObject(result);
    }

    public int GetBuffGoldValue(UnitStatsEnum Stat) =>
        BuffValueCalculator.GetValue(this, Stat, out _, out _, out _);

    public static Buff operator +(Buff a, Buff b)
    {
        if (a.IsInt)
        {
            return new Buff(
            a.Multiplier + b.Multiplier,
            Mathf.RoundToInt(a.baseBuff + b.baseBuff),
            Mathf.RoundToInt(a.additionalBuff + b.additionalBuff));
        }
        else
        {
            return new Buff(
            a.Multiplier + b.Multiplier,
            (float)(a.baseBuff + b.baseBuff),
            (float)(a.additionalBuff + b.additionalBuff));
        }
    }

    public static Buff operator *(Buff a, float multiplier)
    {
        if (a.IsInt)
        {
            return new Buff(
                a.Multiplier * multiplier,
                Mathf.RoundToInt(a.baseBuff * multiplier),
                Mathf.RoundToInt(a.additionalBuff * multiplier)
            );
        }
        else
        {
            return new Buff(
            a.Multiplier * multiplier,
            a.baseBuff * multiplier,
            a.additionalBuff * multiplier);
        }
    }

    public static string CompareString(Buff fromBuff, Buff toBuff)
    {
        string fromBuffBaseBuffPositiveOrNegative = fromBuff.BaseBuff > 0 ? "+" : "";
        string fromBuffMultiplierBuffPositiveOrNegative = fromBuff.Multiplier > 0 ? "+" : "";
        string fromBuffAdditionalBuffPositiveOrNegative = fromBuff.AdditionalBuff > 0 ? "+" : "";

        string toBuffBaseBuffPositiveOrNegative = toBuff.BaseBuff > 0 ? "+" : "";
        string toBuffMultiplierBuffPositiveOrNegative = toBuff.Multiplier > 0 ? "+" : "";
        string toBuffAdditionalBuffPositiveOrNegative = toBuff.AdditionalBuff > 0 ? "+" : "";

        string returnString = "";

        void AddCarageReturn()
        {
            if (returnString != "")
                returnString += "\r\n";
        }

        returnString = fromBuff.BaseBuff != 0 || toBuff.BaseBuff != 0 ? 
            $"Base: {fromBuffBaseBuffPositiveOrNegative}{fromBuff.BaseBuff.Value} -> " +
            $"{toBuffBaseBuffPositiveOrNegative}{toBuff.BaseBuff.Value}" 
            : "";
        AddCarageReturn();
        returnString += fromBuff.Multiplier != 0 || toBuff.Multiplier != 0 ? 
            $"Multiplier: {fromBuffMultiplierBuffPositiveOrNegative}{fromBuff.Multiplier}% -> " +
            $"{toBuffMultiplierBuffPositiveOrNegative}{toBuff.Multiplier}%" 
            : "";
        AddCarageReturn();
        returnString += fromBuff.AdditionalBuff != 0 || toBuff.AdditionalBuff != 0 ? 
            $"Additional: {fromBuffAdditionalBuffPositiveOrNegative}{fromBuff.AdditionalBuff.Value} -> " +
            $"{toBuffAdditionalBuffPositiveOrNegative}{toBuff.AdditionalBuff.Value}" 
            : "";

        return returnString;
    }

    public static string SimpleCompareString(Buff fromBuff, Buff toBuff)
    {
        string fromBuffBaseBuffPositiveOrNegative = fromBuff.BaseBuff > 0 ? "+" : "";
        string fromBuffMultiplierBuffPositiveOrNegative = fromBuff.Multiplier > 0 ? "+" : "";
        string fromBuffAdditionalBuffPositiveOrNegative = fromBuff.AdditionalBuff > 0 ? "+" : "";

        string toBuffBaseBuffPositiveOrNegative = toBuff.BaseBuff > 0 ? "+" : "";
        string toBuffMultiplierBuffPositiveOrNegative = toBuff.Multiplier > 0 ? "+" : "";
        string toBuffAdditionalBuffPositiveOrNegative = toBuff.AdditionalBuff > 0 ? "+" : "";

        string returnString = "";

        void AddCarageReturn()
        {
            if (returnString != "")
                returnString += "\r\n";
        }

        returnString = fromBuff.BaseBuff != 0 || toBuff.BaseBuff != 0 ?
            $"{fromBuffBaseBuffPositiveOrNegative}{fromBuff.BaseBuff.Value} -> " +
            $"{toBuffBaseBuffPositiveOrNegative}{toBuff.BaseBuff.Value}"
            : "";
        AddCarageReturn();
        returnString += fromBuff.Multiplier != 0 || toBuff.Multiplier != 0 ?
            $"{fromBuffMultiplierBuffPositiveOrNegative}{fromBuff.Multiplier}% -> " +
            $"{toBuffMultiplierBuffPositiveOrNegative}{toBuff.Multiplier}%"
            : "";
        AddCarageReturn();
        returnString += fromBuff.AdditionalBuff != 0 || toBuff.AdditionalBuff != 0 ?
            $"{fromBuffAdditionalBuffPositiveOrNegative}{fromBuff.AdditionalBuff.Value} -> " +
            $"{toBuffAdditionalBuffPositiveOrNegative}{toBuff.AdditionalBuff.Value}"
            : "";

        return returnString;
    }

    public string GetDescription()
    {
        string baseBuffPositiveOrNegative = BaseBuff > 0 ? "+" : "";
        string multiplierBuffPositiveOrNegative = Multiplier > 0 ? "+" : "";
        string additionalBuffPositiveOrNegative = AdditionalBuff > 0 ? "+" : "";

        string returnString = "";

        void AddCarageReturn()
        {
            if (returnString != "")
                returnString += "\r\n";
        }

        returnString = BaseBuff != 0 ? $"Base:{baseBuffPositiveOrNegative}{BaseBuff.Value}" : "";
        AddCarageReturn();
        returnString += Multiplier != 0 ? $"Multiplier: {Multiplier}%" : "";
        AddCarageReturn();
        returnString += AdditionalBuff != 0 ? $"Additional: {additionalBuffPositiveOrNegative}{AdditionalBuff.Value}" : "";

        return returnString;
    }

    public override string ToString() =>
        $"Base={BaseBuff}, Add={AdditionalBuff}, Mult={Multiplier}%";
}