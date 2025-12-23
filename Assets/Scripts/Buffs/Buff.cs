using System;
using UnityEngine;

[Serializable]
public class Buff: IBuffSource
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
        set
        {
            baseBuff.IsInt = value;
            additionalBuff.IsInt = value;
        }
    }

    Buff IBuffSource.Buff => this;

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
        float result = ((baseValue + baseBuff) * (1f + Multiplier / 100f) + additionalBuff).FloatValue;



        return baseBuff.IsInt
            ? new IntOrFloatObject(Mathf.RoundToInt(result))
            : new IntOrFloatObject(result);
    }

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

    public static string CompareString(Buff from, Buff to)
    {
        string result = "";

        if (from.BaseBuff != 0 || to.BaseBuff != 0)
        {
            AppendLine(ref result,
                $"Base: {FormatSignedValue(from.BaseBuff)} → {FormatSignedValue(to.BaseBuff)}");
        }

        if (!Mathf.Approximately(from.Multiplier, 0f) || !Mathf.Approximately(to.Multiplier, 0f))
        {
            AppendLine(ref result,
                $"Multiplier: {FormatSignedPercent(from.Multiplier)} → {FormatSignedPercent(to.Multiplier)}");
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

        if (!Mathf.Approximately(from.Multiplier, 0f) || !Mathf.Approximately(to.Multiplier, 0f))
            AppendLine(ref result,
                $"{FormatSignedPercent(from.Multiplier)} → {FormatSignedPercent(to.Multiplier)}");

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

        if (!Mathf.Approximately(Multiplier, 0f))
            AppendLine(ref result, $"Multiplier: {FormatSignedPercent(Multiplier)}");

        if (AdditionalBuff != 0)
            AppendLine(ref result, $"Additional: {FormatSignedValue(AdditionalBuff)}");

        return result;
    }


    public string GetSimpleDescription()
    {
        string result = "";

        if (BaseBuff != 0)
            AppendLine(ref result, FormatSignedValue(BaseBuff));

        if (!Mathf.Approximately(Multiplier, 0f))
            AppendLine(ref result, FormatSignedPercent(Multiplier));

        if (AdditionalBuff != 0)
            AppendLine(ref result, FormatSignedValue(AdditionalBuff));

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

        if (!Mathf.Approximately(from.Multiplier, 0f) || !Mathf.Approximately(to.Multiplier, 0f))
        {
            string text =
                $"{FormatSignedPercent(from.Multiplier)} → {FormatSignedPercent(to.Multiplier)}";

            AppendLine(ref result,
                ArrowCompare(text,
                    from.Multiplier,
                    to.Multiplier));
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

    private static string FormatSignedPercent(float value)
    {
        if (Mathf.Approximately(value, 0f))
            return "0%";

        return value > 0
            ? $"+{value}%"
            : $"{value}%";
    }

    public override string ToString() =>
        $"Base={BaseBuff}, Add={AdditionalBuff}, Mult={Multiplier}%";

}