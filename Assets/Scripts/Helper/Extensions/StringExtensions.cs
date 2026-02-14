using System.Text.RegularExpressions;
using System.Linq;

public static class StringExtensions
{
    public static void AppendLineToTarget(this string line, ref string target)
    {
        if (string.IsNullOrEmpty(line))
            return;

        if (!string.IsNullOrEmpty(target))
            target += "\r\n";

        target += line;
    }

    public static string AddSpacesBeforeCapsRegex(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
    }

    public static string AddStringWithCarriageReturn(this string str, string addStr, int amountOfReturns = 1)
    {
        if (string.IsNullOrEmpty(str))
            return addStr;

        if (string.IsNullOrEmpty(addStr))
        {
            return str;
        }

        string carriageReturns = string.Empty;
        for (int i = 0; i < amountOfReturns; i++)
        {
            carriageReturns += "\r\n";
        }

        return str + carriageReturns + addStr;
    }

    public static void RemoveDuplicates<T>(this System.Collections.Generic.List<T> list)
    {
        if (list == null) return;
        var distinct = System.Linq.Enumerable.Distinct(list).ToList();
        list.Clear();
        list.AddRange(distinct);
    }
}
