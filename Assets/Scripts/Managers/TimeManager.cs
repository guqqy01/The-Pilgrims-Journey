using System.Collections.Generic;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    public readonly HashSet<string> pausedTimeSources = new HashSet<string>();

    public bool IsPaused
    {
        get
        {
            return Mathf.Approximately(Time.timeScale, 0f);
        }
        private set
        {
            if (value)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }

    public void PauseTime(string reference)
    {
        pausedTimeSources.Add(reference);
        IsPaused = true;
    }

    public void RemovePauseRequest(string reference)
    {
        if (!IsPaused || pausedTimeSources.Count > 1)
        {
            pausedTimeSources.Remove(reference);
            return;
        }

        if (pausedTimeSources.Count == 0)
        {
            IsPaused = false;
        }
    }

    public void OverrideUnpauseTime()
    {
        pausedTimeSources.Clear();
        IsPaused = false;
    }
}
