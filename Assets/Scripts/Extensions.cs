using UnityEngine;

public static class Extensions
{
    public static Color SetAlpha(this Color color, float alpha)
    {
        Color holder = new Color(color.r, color.g, color.b, alpha);
        return holder;
    }
}