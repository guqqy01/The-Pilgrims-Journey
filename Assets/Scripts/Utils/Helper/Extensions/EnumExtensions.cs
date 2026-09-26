using System;
using System.Collections.Generic;

public static class EnumExtensions
{
    public static List<T> CollapseFlags<T>(this T flaggedEnum) where T : Enum
    {
        var result = new List<T>();
        long value = Convert.ToInt64(flaggedEnum);

        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            long flagValue = Convert.ToInt64(flag);

            // skip zero-value members and non-power-of-two (composite) members
            if (flagValue == 0 || (flagValue & (flagValue - 1)) != 0)
                continue;

            if ((value & flagValue) == flagValue)
                result.Add(flag);
        }

        return result;
    }
}
