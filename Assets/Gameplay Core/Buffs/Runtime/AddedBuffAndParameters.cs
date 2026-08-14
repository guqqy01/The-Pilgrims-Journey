using System;

public class AddedBuffAndParameters
{
    public IBuffSource BuffSource { get; set; }
    public AddBuffParameters AddBuffParameters { get; set; }

    public Buff GetBuff()
    {
        if (BuffSource == null)
            return null;

        Buff buff = null;
        try
        {
            buff = BuffSource.Buff;
        }
        catch (Exception)
        {
            // If retrieving the buff causes recursion or other issues, treat as no buff
            buff = null;
        }

        // If we couldn't get a Buff object but the source is a BuffableStat, create a snapshot Buff from its current Value
        if (buff == null && BuffSource is BuffableStat bs)
        {
            try
            {
                var val = bs.Value;
                // Create a simple Buff that represents the current numeric value as baseBuff (no multiplier)
                return new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, 0f, val, val.IsInt ? new IntOrFloatObject(0) : new IntOrFloatObject(0f));
            }
            catch
            {
                return null;
            }
        }

        if (AddBuffParameters == AddBuffParameters.All)
            return buff;

        bool isInt = buff?.IsInt ?? false;

        float multiplier = 0f;
        IntOrFloatObject baseBuff = new IntOrFloatObject(isInt ? 0 : 0f);
        IntOrFloatObject additionalBuff = new IntOrFloatObject(isInt ? 0 : 0f);

        if (buff != null && AddBuffParameters.HasFlag(AddBuffParameters.Multiplier))
        {
            multiplier = buff.Multiplier;
        }

        if (buff != null && AddBuffParameters.HasFlag(AddBuffParameters.Base))
        {
            baseBuff = buff.BaseBuff;
        }

        if (buff != null && AddBuffParameters.HasFlag(AddBuffParameters.Additional))
        {
            additionalBuff = buff.AdditionalBuff;
        }

        return new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, multiplier, baseBuff, additionalBuff);
    }

    public AddedBuffAndParameters(IBuffSource buffSource, AddBuffParameters addBuffParameters = AddBuffParameters.All)
    {
        BuffSource = buffSource;
        AddBuffParameters = addBuffParameters;
    }
}
