using System.Collections;
using UnityEngine;

public class ThemeManager : Singleton<ThemeManager>
{
    [SerializeField]
    private AudioSource mainMusicThemeSource;

    [Tooltip("Fade duration when switching between themes entirely.")]
    [SerializeField]
    private float switchThemeTime;

    [Tooltip("Fade duration between consecutive songs within the same theme.")]
    [SerializeField]
    private float songFadeTime;

    private Theme _currentlyPlayingTheme;

    // The ONE coroutine allowed to touch mainMusicThemeSource at any given time.
    // Every public command (Play/Stop) routes through RunExclusive, which guarantees
    // whatever was previously running here gets stopped first. This is what makes
    // rapid-fire calls safe — there is never a second, untracked coroutine left
    // fighting over the source's volume in the background.
    private Coroutine _activeCoroutine;
    private bool _sourceRegistered;

    public Theme CurrentTheme => _currentlyPlayingTheme;
    public bool IsPlayingTheme(Theme theme) => _currentlyPlayingTheme == theme && _activeCoroutine != null;

    private void Awake()
    {
        InitiateFromRootAtAwake();
    }

    /// <param name="forceRestart">If true, restarts the theme from its first song even if it's already playing.</param>
    public void PlayTheme(Theme theme, bool forceRestart = false)
    {
        if (theme == null)
        {
            StopTheme();
            return;
        }

        if (!forceRestart && IsPlayingTheme(theme))
            return;

        RunExclusive(SwitchThemes(theme));
    }

    /// <summary>Fades everything out and stops music entirely (e.g. for a cutscene or menu).</summary>
    public void StopTheme(float? fadeOutOverride = null)
    {
        _currentlyPlayingTheme = null;
        RunExclusive(FadeOutAndStop(fadeOutOverride ?? switchThemeTime));
    }

    /// <summary>Cancels whatever's currently running on mainMusicThemeSource, then starts the new routine as the sole owner.</summary>
    private void RunExclusive(IEnumerator routine)
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);

        _activeCoroutine = StartCoroutine(routine);
    }

    private IEnumerator FadeOutAndStop(float fadeTime)
    {
        if (mainMusicThemeSource.isPlaying)
            yield return AudioManager.Instance.FadeOutAudioOverTime(mainMusicThemeSource, fadeTime);

        _activeCoroutine = null;
    }

    private IEnumerator SwitchThemes(Theme theme)
    {
        if (mainMusicThemeSource.isPlaying)
            yield return AudioManager.Instance.FadeOutAudioOverTime(mainMusicThemeSource, switchThemeTime);

        // Nested directly into this same coroutine rather than a separately-tracked
        // one — there's only ever one owner of mainMusicThemeSource, full stop.
        yield return CycleThemeSongs(theme, switchThemeTime);
    }

    private IEnumerator CycleThemeSongs(Theme theme, float? initialFadeInOverride = null)
    {
        if (theme == null || theme.Songs == null || theme.Songs.Count == 0)
        {
            _activeCoroutine = null;
            yield break;
        }

        _currentlyPlayingTheme = theme;

        int currentIndex = 0;
        bool isFirstSong = true;

        while (true)
        {
            float fadeIn = (isFirstSong && initialFadeInOverride.HasValue) ? initialFadeInOverride.Value : songFadeTime;
            yield return PlayAndFadeSong(theme.Songs[currentIndex], fadeIn, songFadeTime);

            isFirstSong = false;
            currentIndex = (currentIndex + 1) % theme.Songs.Count;
        }
    }

    /// <summary>Plays one clip on the shared source: fade in, hold, fade out over its tail — then returns.</summary>
    private IEnumerator PlayAndFadeSong(AudioClipAndSettings settings, float fadeInTime, float fadeOutTime)
    {
        float clipLength = settings.clip.length;
        float safeFadeIn = Mathf.Min(fadeInTime, clipLength / 2f);
        float safeFadeOut = Mathf.Min(fadeOutTime, clipLength / 2f);
        float targetVolume = settings.volume * AudioManager.Instance.GetTotalAudioTypeVolume(settings.Type);

        mainMusicThemeSource.Setup(settings);

        EnsureSourceRegistered(settings);
        mainMusicThemeSource.Play();

        yield return AudioManager.Instance.FadeInAudioOverTime(mainMusicThemeSource, targetVolume, safeFadeIn);

        float holdTime = clipLength - safeFadeIn - safeFadeOut;
        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);

        yield return AudioManager.Instance.FadeOutAudioOverTime(mainMusicThemeSource, safeFadeOut);
    }

    private void EnsureSourceRegistered(AudioClipAndSettings settings)
    {
        if (!_sourceRegistered)
        {
            AudioManager.Instance.RegisterExternalSource(mainMusicThemeSource, settings);
            _sourceRegistered = true;
        }
        else
        {
            AudioManager.Instance.UpdateExternalSource(mainMusicThemeSource, settings);
        }
    }
}