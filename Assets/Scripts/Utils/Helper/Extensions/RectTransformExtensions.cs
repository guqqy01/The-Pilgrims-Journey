using System.Collections.Generic;
using UnityEngine;

public static class RectTransformExtensions
{
    public static List<RectTransform> GetChildrenAtDepth(this RectTransform parent, int depth)
    {
        if (depth <= 0) return null;

        List<RectTransform> childrenAtDepth = new List<RectTransform>();

        for (int i = 0; i < parent.childCount; i++)
        {
            RectTransform child = (RectTransform)parent.GetChild(i);
            if (child.GetDepth(parent) == depth)
            {
                childrenAtDepth.Add(child);
            }
        }

        return childrenAtDepth;
    }

    private static int GetDepth(this Transform t, Transform parent)
    {
        int depth = 1;
        while (t.parent != null && t.parent != parent)
        {
            depth++;
            t = t.parent;
        }
        return depth;
    }
}
