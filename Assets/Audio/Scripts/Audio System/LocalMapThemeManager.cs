using GameManagerCalls;
using System.Collections;
using UnityEngine;

public enum LocalMapMusicState
{
    Relax,
    Play,
    Escape
}

public class LocalMapThemeManager : Singleton<LocalMapThemeManager>, IOnAfterCreateLocalMap, IOnPlayLocalLevel
{
    private LocalMapTheme _mapTheme;

    [Tooltip("How much of the impact stinger plays before the new loop kicks in. " +
             "0 = loop starts the instant the stinger fires (overlapping). " +
             "Set to the stinger's length for a fully sequential hit-then-loop.")]
    [SerializeField]
    private float impactLeadInTime = 0.5f;

    private LocalMapMusicState? _currentState;
    private Coroutine _transitionCoroutine;

    public LocalMapMusicState? CurrentState => _currentState;
    public LocalMapTheme CurrentMapTheme => _mapTheme;
    
    /// <summary>
    /// Swap to a new map's theme set. This is a location change, not a state transition —
    /// it never plays an impact stinger, it just crossfades straight into the chosen
    /// starting state (defaults to Relax, since whatever state you were in belonged to
    /// the map you're leaving).
    /// </summary>
    public void ChangeMapTheme(LocalMapTheme newMapTheme, LocalMapMusicState initialState = LocalMapMusicState.Relax)
    {
        if (newMapTheme == null || newMapTheme == _mapTheme)
            return;

        _mapTheme = newMapTheme;

        if (_transitionCoroutine != null)
            StopCoroutine(_transitionCoroutine);

        _currentState = initialState;
        ThemeManager.Instance.PlayTheme(GetTheme(initialState));
    }

    public void SetState(LocalMapMusicState newState, bool forceRestart = false)
    {
        if (_mapTheme == null)
        {
            Debug.LogWarning("LocalMapThemeManager: SetState called before a map theme was assigned via ChangeMapTheme.");
            return;
        }

        if (!forceRestart && _currentState == newState)
            return;

        if (_transitionCoroutine != null)
            StopCoroutine(_transitionCoroutine);

        _transitionCoroutine = StartCoroutine(TransitionTo(newState));
    }

    private IEnumerator TransitionTo(LocalMapMusicState newState)
    {
        AudioClipAndSettings impact = GetImpactClip(_currentState, newState);
        _currentState = newState;

        if (impact != null)
        {
            AudioManager.Instance.PlayClip(impact);
            if (impactLeadInTime > 0f)
                yield return new WaitForSeconds(impactLeadInTime);
        }

        ThemeManager.Instance.PlayTheme(GetTheme(newState));
    }

    private Theme GetTheme(LocalMapMusicState state) => state switch
    {
        LocalMapMusicState.Relax => _mapTheme.Relax,
        LocalMapMusicState.Play => _mapTheme.Play,
        LocalMapMusicState.Escape => _mapTheme.Escape,
        _ => null
    };

    // Only forward transitions have a defined stinger right now (matches the two clips
    // on LocalMapTheme). Anything else — including reverse transitions and the very
    // first state set — just crossfades straight into the new loop via ThemeManager.
    private AudioClipAndSettings GetImpactClip(LocalMapMusicState? from, LocalMapMusicState to)
    {
        if (from == LocalMapMusicState.Relax && to == LocalMapMusicState.Play)
            return _mapTheme.RelaxToPlayImpact;

        if (from == LocalMapMusicState.Play && to == LocalMapMusicState.Escape)
            return _mapTheme.PlayToEscapeImpact;

        return null;
    }

    public void AfterCreateLocalMap()
    {
        SetState(LocalMapMusicState.Relax);
    }

    public void OnPlayLocalLevel()
    {
        SetState(LocalMapMusicState.Play);
    }
}