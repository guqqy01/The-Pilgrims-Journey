using System;
using System.Collections.Generic;

public static class EnumExtensions
{
    public static List<T> CollapseFlags<T>(this T flaggedEnum) where T : Enum
    {
        List<T> returnList = new List<T>();

        string[] names = typeof(T).GetEnumNames();

        foreach(string name in names)
        {
            if (Enum.TryParse(typeof(T), name, out object result))
            {
                if (result is not T flag)
                    continue;

                if (flaggedEnum.HasFlag(flag))
                {
                    returnList.Add(flag);
                }
            }
        }

        return returnList;
    }
}
