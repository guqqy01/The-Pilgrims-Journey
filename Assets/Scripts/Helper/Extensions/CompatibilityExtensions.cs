using UnityEngine;

/// <summary>
/// Compatibility shim to avoid changing many call sites that referenced the old ExtensionsAndHelper static methods.
/// Contains a RandomSign method used in other scripts.
/// </summary>
public static class CompatibilityExtensions
{
    public static int RandomSign()
    {
        var holder = new System.Collections.Generic.List<int>() { -1, 1 };
        return holder[UnityEngine.Random.Range(0, holder.Count)];
    }

    public static int RandomSignInt(this int value)
    {
        return value * RandomSign();
    }

    public static float RandomSignFloat(this float value)
    {
        return value * (RandomSign() == 1 ? 1f : -1f);
    }
}
