using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    private AudioSource mainMusicThemeSource;

    private List<AudioSource> audioSources = new List<AudioSource>();

    [Header("UI")]
    [SerializeField] private AudioClipAndSettings buttonClick;
    [SerializeField] private AudioClipAndSettings openWindow;
    [SerializeField] private AudioClipAndSettings closeWindow;
    [Header("Tiles")]
    [SerializeField] private AudioClipAndSettings selectTile;
    [SerializeField] private AudioClipAndSettings deselectTile;
    [SerializeField] private AudioClipAndSettings pickupTile;
    [SerializeField] private AudioClipAndSettings moveTile;
    [SerializeField] private AudioClipAndSettings cancelMoveTile;
    [SerializeField] private AudioClipAndSettings placeTile;
    [SerializeField] private AudioClipAndSettings rotateTile;
    [Header("Equipment")]
    [SerializeField] private AudioClipAndSettings selectEquipment;
    public void PlayButtonClick() => buttonClick.Play(UIVolume);
    public void PlayOpenWindow() => openWindow.Play(UIVolume);
    public void PlayCloseWindow() => closeWindow.Play(UIVolume);
    public void PlaySelectTile() => selectTile.Play(UIVolume);
    public void PlayDeselectTile() => deselectTile.Play(UIVolume);
    public void PlayPickupTile() => pickupTile.Play(UIVolume);
    public void PlayMoveTile() => moveTile.Play(UIVolume);
    public void PlayCancelMoveTile() => cancelMoveTile.Play(UIVolume);
    public void PlayPlaceTile() => placeTile.Play(UIVolume);
    public void PlayRotateTile() => rotateTile.Play(UIVolume);
    public void PlaySelectEquipment() => selectEquipment.Play(UIVolume);

    [Range(0f, 1f)]
    public float UIVolume = 0.5f;
    
    [SerializeField]
    private List<AudioClipAndSettings> mainThemes;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogWarning("Another instance of AudioManager found");
    }

    private void Start()
    {
        if(mainThemes.Count > 0)
            StartCoroutine(CycleMainThemes());
    }

    private IEnumerator CycleMainThemes()
    {
        int currentIndex = 0;
        while (true)
        {
            if (!mainMusicThemeSource.isPlaying)
            {
                SetUpAudioSource(mainMusicThemeSource, mainThemes[currentIndex]);
                mainMusicThemeSource.Play();
                currentIndex = (currentIndex + 1) % mainThemes.Count;
            }

            yield return null;
        }
    }

    public AudioSource PlayClipOnObject(AudioClipAndSettings clip, GameObject obj)
    {
        AudioSource source = obj.AddComponent<AudioSource>();
        SetUpAudioSource(source, clip);
        source.Play();

        if (!clip.loop)
            StartCoroutine(WaitForEndOfAudio(source));

        return source;
    }

    public AudioSource PlayClip(AudioClipAndSettings clip)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        SetUpAudioSource(source, clip);
        audioSources.Add(source);
        source.Play();

        if(!clip.loop)
            StartCoroutine(WaitForEndOfAudio(source));

        return source;
    }

    public AudioSource PlayClip(AudioClipAndSettings clip, float overrideVolume)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        SetUpAudioSource(source, clip, overrideVolume);
        audioSources.Add(source);
        source.Play();

        if (!clip.loop)
            StartCoroutine(WaitForEndOfAudio(source));

        return source;
    }

    private void SetUpAudioSource(AudioSource source, AudioClipAndSettings settings)
    {
        source.clip = settings.clip;
        source.loop = settings.loop;
        source.volume = settings.volume;
        if (settings.pitch == 0)
            source.pitch = 1;
        else
            source.pitch = settings.pitch;
    }

    private void SetUpAudioSource(AudioSource source, AudioClipAndSettings settings, float overrideVolume)
    {
        source.clip = settings.clip;
        source.loop = settings.loop;
        source.volume = overrideVolume;
        if (settings.pitch == 0)
            source.pitch = 1;
        else
            source.pitch = settings.pitch;
    }

    public void StopLoopingAudioClipOnceFinished(AudioSource source)
    {
        if (!source.loop)
        {
            StartCoroutine(WaitForEndOfAudio(source));
            return;
        }

        source.loop = false;

        StartCoroutine(WaitForEndOfAudio(source));
    }

    public IEnumerator FadeOutAudioOverTime(AudioSource source, float seconds)
    {
        float startTime = Time.time;
        float finishTime = Time.time + seconds;

        float startVolume = source.volume;

        while (true)
        {
            float t = 1f - Mathf.Clamp01(Mathf.InverseLerp(startTime, finishTime, Time.time));

            source.volume = startVolume * t;

            if (t <= 0.01f)
            {
                audioSources.Remove(source);
                Destroy(source);
                break;
            }

            yield return null;
        }
    }

    private IEnumerator WaitForEndOfAudio(AudioSource source)
    {
        while (source.isPlaying)
        {
            yield return null;

            if (source == null)
            {
                audioSources.RemoveAll(x => x == null);
                break;
            }
        }

        if(source != null)
        {
            audioSources.Remove(source);
            Destroy(source);
        }
    }
}

[Serializable]
public class AudioClipAndSettings
{
    public AudioClip clip;
    public bool loop = false;
    [Range(0, 1)]
    public float volume = 1;
    [Range(-3, 3)]
    public float pitch = 1;

    public AudioClipAndSettings(AudioClipAndSettings audioClipAndSettings) 
    {
        clip = audioClipAndSettings.clip;
        loop = audioClipAndSettings.loop;
        volume = audioClipAndSettings.volume;
        pitch = audioClipAndSettings.pitch;
    }

    public void Play()
    {
        AudioManager.Instance.PlayClip(this);
    }

    public void Play(float setVolume)
    {
        AudioManager.Instance.PlayClip(this, volume * setVolume);
    }
}
