using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    private readonly HashSet<string> _pausedTimeSources = new HashSet<string>();

    private readonly Dictionary<string, float> _timeScaleSources = new Dictionary<string, float>();

    [SerializeField]
    private float timeScaleTransitionRate = 0.5f; // units of timeScale per real second

    private Coroutine _changeTimeScaleCoroutine;

    public bool IsPaused
    {
        get
        {
            return Mathf.Approximately(Time.timeScale, 0f);
        }
        private set
        {
            if (_changeTimeScaleCoroutine != null)
                StopCoroutine(_changeTimeScaleCoroutine);

            if (value)
            {
                Time.timeScale = 0f;
                _changeTimeScaleCoroutine = null;
            }
            else
            {
                Time.timeScale = GetSlowestTimeScale();
                _changeTimeScaleCoroutine = null;
            }
        }
    }

    private float GetSlowestTimeScale()
    {
        if (_timeScaleSources.Count == 0)
            return 1f;

        return _timeScaleSources.Values.Min();
    }

    public void AddTimeScaleReference(string reference, float timeScale)
    {
        if (_timeScaleSources.ContainsKey(reference))
            _timeScaleSources[reference] = timeScale;
        else _timeScaleSources.Add(reference, timeScale);

        if (IsPaused)
            return;

        float slowestTimeScale = GetSlowestTimeScale();
        float currentScale = Time.timeScale;

        if (Mathf.Approximately(slowestTimeScale, currentScale))
            return;

        if (_changeTimeScaleCoroutine != null)
            StopCoroutine(_changeTimeScaleCoroutine);

        _changeTimeScaleCoroutine = StartCoroutine(ChangeTimeScale(slowestTimeScale));
    }

    // Ramps Time.timeScale toward targetTimeScale at a fixed rate per real (unscaled) second.
    // Mathf.MoveTowards naturally handles both increasing and decreasing cases correctly.
    private IEnumerator ChangeTimeScale(float targetTimeScale)
    {
        while (!Mathf.Approximately(Time.timeScale, targetTimeScale))
        {
            Time.timeScale = Mathf.MoveTowards(
                Time.timeScale,
                targetTimeScale,
                timeScaleTransitionRate * Time.unscaledDeltaTime);

            yield return null;
        }

        Time.timeScale = targetTimeScale;
        _changeTimeScaleCoroutine = null;
    }

    public void RemoveTimeScaleReference(string reference)
    {
        if (!_timeScaleSources.ContainsKey(reference))
            return;

        _timeScaleSources.Remove(reference);

        if (IsPaused)
            return;

        float slowestTimeScale = GetSlowestTimeScale();
        float currentScale = Time.timeScale;

        if (Mathf.Approximately(slowestTimeScale, currentScale))
            return;

        if (_changeTimeScaleCoroutine != null)
            StopCoroutine(_changeTimeScaleCoroutine);

        _changeTimeScaleCoroutine = StartCoroutine(ChangeTimeScale(slowestTimeScale));
    }

    public void PauseTime(string reference)
    {
        _pausedTimeSources.Add(reference);
        IsPaused = true;
    }

    public void RemovePauseRequest(string reference)
    {
        _pausedTimeSources.Remove(reference);

        if (!IsPaused || _pausedTimeSources.Count >= 1)
        {
            return;
        }

        IsPaused = false;
    }

    public void OverrideUnpauseTime()
    {
        _pausedTimeSources.Clear();
        _timeScaleSources.Clear();
        IsPaused = false;
    }
}