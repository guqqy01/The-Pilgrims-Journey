using System.Collections.Generic;
using UnityEngine;

public static class DestroyExtensions
{
    public static void DestroyAndClearList<T>(this List<T> list) where T : MonoBehaviour
    {
        if (list == null) return;

        foreach (T item in list)
        {
            if (item == null)
                continue;

            if (Application.isPlaying)
            {
                GameObject.Destroy(item.gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(item.gameObject);
            }
        }

        list.Clear();
    }

    public static void DestroyAndClearList(this List<GameObject> list)
    {
        if (list == null) return;

        foreach (GameObject item in list)
        {
            if (item == null)
                continue;

            if (Application.isPlaying)
            {
                GameObject.Destroy(item);
            }
            else
            {
                GameObject.DestroyImmediate(item);
            }
        }

        list.Clear();
    }
}
