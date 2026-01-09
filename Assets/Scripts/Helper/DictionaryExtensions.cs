using System.Collections.Generic;
// Small helper extensions
public static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(
        this Dictionary<TKey, TValue> dict,
        TKey key)
    {
        dict.TryGetValue(key, out TValue v);
        return v;
    }
}
