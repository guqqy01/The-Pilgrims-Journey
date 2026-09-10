using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Buff: IBuffSource, ISerializationCallbackReceiver
{
    public BuffType buffFlags = BuffType.Base;
    [SerializeField]
    [HideInInspector]
    private bool initialized;

    [SerializeField]
    [FormerlySerializedAs("multiplier")]
    [Tooltip("Percentage multiplier. Example: 100 = +100% (doubles the base).")]
    private float percentageMultiplier = 0f;

    [SerializeField] private IntOrFloatObject baseBuff;
    [SerializeField] private IntOrFloatObject additionalBuff;

    public static string NameOfMultiplier => nameof(percentageMultiplier);
    public static string NameOfBase => nameof(baseBuff);
    public static string NameOfAdditional => nameof(additionalBuff);
    /// <summary>
    /// Percentage multiplier. Example: 100 = +100% (doubles the base).
    /// </summary>
    public float PercentageMultiplier
    {
        get
        {
            if(buffFlags.HasFlag(BuffType.Multiplier))
                return percentageMultiplier;

            return 0f;
        }
        set
        {
            if (buffFlags.HasFlag(BuffType.Multiplier))
                percentageMultiplier = value;
        }
    }
    public IntOrFloatObject BaseBuff
    {
        get
        {
            if (buffFlags.HasFlag(BuffType.Base))
                return baseBuff;

            return IsInt ? 0 : 0f;
        }
        set
        {
            if (buffFlags.HasFlag(BuffType.Base))
                baseBuff = value;
        }
    }
    public IntOrFloatObject AdditionalBuff
    {
        get
        {
            if (buffFlags.HasFlag(BuffType.Additional))
                return additionalBuff;

            return IsInt ? 0 : 0f;
        }
        set
        {
            if (buffFlags.HasFlag(BuffType.Additional))
                additionalBuff = value;
        }
    }

    public bool IsInt
    {
        get
        {
            return baseBuff.IsInt;
        }
        set
        {
            baseBuff.IsInt = value;
            additionalBuff.IsInt = value;
        }
    }

    Buff IBuffSource.Buff => this;

    public Buff(BuffType buffType, bool isInt)
    {
        buffFlags = buffType;

        PercentageMultiplier = 0f;

        if (isInt)
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

    public Buff(BuffType buffType, float multiplier, IntOrFloatObject baseBuff, IntOrFloatObject additionalBuff)
    {
        buffFlags = buffType;
        IsInt = baseBuff.IsInt;
        PercentageMultiplier = multiplier;
        BaseBuff = baseBuff;
        AdditionalBuff = additionalBuff;
    }

    /// <summary>
    /// (baseValue + BaseBuff) * (1 + Multiplier/100) + AdditionalBuff
    /// </summary>
    public virtual IntOrFloatObject Value(IntOrFloatObject baseValue)
    {
        float result = ((baseValue + BaseBuff) * (1f + PercentageMultiplier / 100f) + AdditionalBuff).FloatValue;

        return IsInt
            ? new IntOrFloatObject(Mathf.RoundToInt(result))
            : new IntOrFloatObject(result);
    }

    public static Buff operator +(Buff a, Buff b)
    {
        if (a == null && b == null)
            return null;

        if(a == null)
        {
            return b;
        }

        if(b == null)
        {
            return a;
        }


        BuffType buffType = a.buffFlags | b.buffFlags;
        if (a.IsInt)
        {
            return new Buff(
                buffType,
                a.PercentageMultiplier + b.PercentageMultiplier,
                Mathf.RoundToInt(a.BaseBuff + b.BaseBuff),
                Mathf.RoundToInt(a.AdditionalBuff + b.AdditionalBuff));
        }
        else
        {
            return new Buff(
                buffType,
                a.PercentageMultiplier + b.PercentageMultiplier,
                (float)(a.BaseBuff + b.BaseBuff),
                (float)(a.AdditionalBuff + b.AdditionalBuff));
        }
    }

    public static Buff operator *(Buff a, float multiplier)
    {
        if (a.IsInt)
        {
            return new Buff(
                a.buffFlags,
                a.PercentageMultiplier * multiplier,
                Mathf.RoundToInt(a.BaseBuff * multiplier),
                Mathf.RoundToInt(a.AdditionalBuff * multiplier)
            );
        }
        else
        {
            return new Buff(
                a.buffFlags,
                a.PercentageMultiplier * multiplier,
                a.BaseBuff * multiplier,
                a.AdditionalBuff * multiplier);
        }
    }

    public static string CompareString(Buff from, Buff to)
    {
        string result = "";

        if (from.BaseBuff != 0 || to.BaseBuff != 0)
        {
            AppendLine(ref result,
                $"Base: {FormatSignedValue(from.BaseBuff)} → {FormatSignedValue(to.BaseBuff)}");
        }

        if (!Mathf.Approximately(from.PercentageMultiplier, 0f) || !Mathf.Approximately(to.PercentageMultiplier, 0f))
        {
            AppendLine(ref result,
                $"Multiplier: {FormatSignedPercent(from.PercentageMultiplier)} → {FormatSignedPercent(to.PercentageMultiplier)}");
        }

        if (from.AdditionalBuff != 0 || to.AdditionalBuff != 0)
        {
            AppendLine(ref result,
                $"Additional: {FormatSignedValue(from.AdditionalBuff)} → {FormatSignedValue(to.AdditionalBuff)}");
        }

        return result;
    }

    public static string SimpleCompareString(Buff from, Buff to)
    {
        string result = "";

        if (from.BaseBuff != 0 || to.BaseBuff != 0)
            AppendLine(ref result,
                $"{FormatSignedValue(from.BaseBuff)} → {FormatSignedValue(to.BaseBuff)}");

        if (!Mathf.Approximately(from.PercentageMultiplier, 0f) || !Mathf.Approximately(to.PercentageMultiplier, 0f))
            AppendLine(ref result,
                $"{FormatSignedPercent(from.PercentageMultiplier)} → {FormatSignedPercent(to.PercentageMultiplier)}");

        if (from.AdditionalBuff != 0 || to.AdditionalBuff != 0)
            AppendLine(ref result,
                $"{FormatSignedValue(from.AdditionalBuff)} → {FormatSignedValue(to.AdditionalBuff)}");

        return result;
    }

    public string GetDescription()
    {
        string result = "";

        if (BaseBuff != 0)
            AppendLine(ref result, $"Base: {FormatSignedValue(BaseBuff)}");

        if (!Mathf.Approximately(PercentageMultiplier, 0f))
            AppendLine(ref result, $"Multiplier: {FormatSignedPercent(PercentageMultiplier)}");

        if (AdditionalBuff != 0)
            AppendLine(ref result, $"Additional: {FormatSignedValue(AdditionalBuff)}");

        return result;
    }


    public string GetSignedSimpleDescription()
    {
        string result = "";

        if (BaseBuff != 0)
            AppendLine(ref result, FormatSignedValue(BaseBuff));

        if (!Mathf.Approximately(PercentageMultiplier, 0f))
            AppendLine(ref result, FormatSignedPercent(PercentageMultiplier));

        if (AdditionalBuff != 0)
            AppendLine(ref result, FormatSignedValue(AdditionalBuff));

        return result;
    }

    public string GetSimpleDescription()
    {
        string result = "";

        if (BaseBuff != 0)
            AppendLine(ref result, FormatValue(BaseBuff));

        if (!Mathf.Approximately(PercentageMultiplier, 0f))
            AppendLine(ref result, FormatSignedPercent(PercentageMultiplier));

        if (AdditionalBuff != 0)
            AppendLine(ref result, FormatValue(AdditionalBuff));

        return result;
    }

    public static string CompareStringColoredEditor(Buff from, Buff to)
    {
        string result = "";

        if (from.BaseBuff != 0 || to.BaseBuff != 0)
        {
            string text =
                $"{FormatSignedValue(from.BaseBuff)} → {FormatSignedValue(to.BaseBuff)}";

            AppendLine(ref result,
                ArrowCompare(text,
                    from.BaseBuff.FloatValue,
                    to.BaseBuff.FloatValue));
        }

        if (!Mathf.Approximately(from.PercentageMultiplier, 0f) || !Mathf.Approximately(to.PercentageMultiplier, 0f))
        {
            string text =
                $"{FormatSignedPercent(from.PercentageMultiplier)} → {FormatSignedPercent(to.PercentageMultiplier)}";

            AppendLine(ref result,
                ArrowCompare(text,
                    from.PercentageMultiplier,
                    to.PercentageMultiplier));
        }

        if (from.AdditionalBuff != 0 || to.AdditionalBuff != 0)
        {
            string text =
                $"{FormatSignedValue(from.AdditionalBuff)} → {FormatSignedValue(to.AdditionalBuff)}";

            AppendLine(ref result,
                ArrowCompare(text,
                    from.AdditionalBuff.FloatValue,
                    to.AdditionalBuff.FloatValue));
        }

        return result;
    }


    private static string ArrowCompare(string fromText, float from, float to)
    {
        if (Mathf.Approximately(from, to))
        {
            return $"<color=#AAAAAA> {fromText}</color>"; // unchanged
        }

        if (to > from)
        {
            return $"<color=#4CAF50>▲ {fromText}</color>"; // increased
        }

        return $"<color=#F44336>▼ {fromText}</color>";     // decreased
    }

    private static void AppendLine(ref string target, string line)
    {
        if (string.IsNullOrEmpty(line))
            return;

        if (!string.IsNullOrEmpty(target))
            target += "\r\n";

        target += line;
    }

    private static string FormatSignedValue(IntOrFloatObject value)
    {
        if (value == 0)
            return "0";

        return value > 0
            ? $"+{value.Value}"
            : value.Value.ToString();
    }

    private static string FormatValue(IntOrFloatObject value)
    {
        return value.Value.ToString();
    }

    private static string FormatSignedPercent(float value)
    {
        if (Mathf.Approximately(value, 0f))
            return "0%";

        return value > 0
            ? $"+{value}%"
            : $"{value}%";
    }

    public override string ToString() =>
        $"Base={BaseBuff}, Add={AdditionalBuff}, Mult={PercentageMultiplier}%";

    public void OnBeforeSerialize()
    {
        if (!initialized)
        {
            buffFlags = BuffType.Base;
            initialized = true;
        }
    }

    public void OnAfterDeserialize()
    {
        
    }

    public static implicit operator IntOrFloatObject(Buff value) => value.Value(value.IsInt ? 0 : 0f);
}