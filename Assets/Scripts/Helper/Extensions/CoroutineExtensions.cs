using System;
using System.Collections;
using UnityEngine;

public static class CoroutineExtensions
{
    public static IEnumerator WaitForObjectNotNullThenPerformAction(object objectToWaitFor, Action action, float maxTimeToWait = 60)
    {
        float startTime = Time.time;

        while (objectToWaitFor == null && Time.time < startTime + maxTimeToWait)
        {
            yield return null;
        }

        if (objectToWaitFor != null)
            action?.Invoke();
    }
}
