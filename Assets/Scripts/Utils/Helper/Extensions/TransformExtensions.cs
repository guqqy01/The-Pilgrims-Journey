using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static List<Transform> GetChildrenAtDepth(this Transform parent, int depth)
    {
        if (depth <= 0) return null;

        List<Transform> childrenAtDepth = new List<Transform>();

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
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
