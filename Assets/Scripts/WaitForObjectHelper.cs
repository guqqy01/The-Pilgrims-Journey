using System;
using System.Collections;
using UnityEngine;

public static class WaitForObjectHelper
{
    public static IEnumerator WaitForObject(Func<UnityEngine.Object> getObject, Action onReady, float checkInterval = 0f)
    {
        if (checkInterval <= 0f)
        {
            // Poll every frame until non-null
            yield return new WaitUntil(() => getObject() != null);
        }
        else
        {
            // Poll at fixed intervals
            while (getObject() == null)
            {
                yield return new WaitForSeconds(checkInterval);
            }
        }

        onReady?.Invoke();
    }


    public static IEnumerator WaitForObject<T>(Func<UnityEngine.Object> getObject, Action<T> onReady, T onReadyObj, float checkInterval = 0f)
    {
        if (checkInterval <= 0f)
        {
            // Poll every frame until non-null
            yield return new WaitUntil(() => getObject() != null);
        }
        else
        {
            // Poll at fixed intervals
            while (getObject() == null)
            {
                yield return new WaitForSeconds(checkInterval);
            }
        }

        onReady?.Invoke(onReadyObj);
    }
}