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
    private Coroutine _switchCoroutine;
    private Coroutine _cycleCoroutine;
    private bool _sourceRegistered;

    public Theme CurrentTheme => _currentlyPlayingTheme;
    public bool IsPlayingTheme(Theme theme) => _currentlyPlayingTheme == theme && _cycleCoroutine != null;

    private void Awake()
    {
        InitiateFromRootAtAwake();
    }

    /// <param name="forceRestart">If true, restarts the theme from its first song even if it's already playing.</param>
    public void PlayTheme(Theme theme, bool forceRestart = false)
    {
        if (!forceRestart && IsPlayingTheme(theme))
            return;

        if (_switchCoroutine != null)
            StopCoroutine(_switchCoroutine);

        _switchCoroutine = StartCoroutine(SwitchThemes(theme));
    }

    /// <summary>Fades everything out and stops music entirely (e.g. for a cutscene or menu).</summary>
    public void StopTheme(float? fadeOutOverride = null)
    {
        if (_switchCoroutine != null) StopCoroutine(_switchCoroutine);
        if (_cycleCoroutine != null) StopCoroutine(_cycleCoroutine);

        _cycleCoroutine = null;
        _currentlyPlayingTheme = null;

        if (mainMusicThemeSource.isPlaying)
            StartCoroutine(AudioManager.Instance.FadeOutAudioOverTime(mainMusicThemeSource, fadeOutOverride ?? switchThemeTime));
    }

    private IEnumerator SwitchThemes(Theme theme)
    {
        if (_cycleCoroutine != null)
        {
            StopCoroutine(_cycleCoroutine);
            yield return AudioManager.Instance.FadeOutAudioOverTime(mainMusicThemeSource, switchThemeTime);
        }

        // First song of a new theme fades in over switchThemeTime (a deliberate transition),
        // subsequent songs within the theme use the shorter songFadeTime.
        _cycleCoroutine = StartCoroutine(CycleThemeSongs(theme, switchThemeTime));
    }

    private IEnumerator CycleThemeSongs(Theme theme, float? initialFadeInOverride = null)
    {
        if (theme == null || theme.Songs == null || theme.Songs.Count == 0)
            yield break;

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

        AudioManager.Instance.SetUpAudioSource(mainMusicThemeSource, settings);
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