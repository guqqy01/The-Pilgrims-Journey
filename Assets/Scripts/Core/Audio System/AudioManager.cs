using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    private AudioSource mainMusicThemeSource;

    private readonly AudioManagerSourcesContainer audioSourceContainer = new AudioManagerSourcesContainer();

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 8;
    private readonly Queue<AudioSource> pooledSources = new Queue<AudioSource>();

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
    public void PlayButtonClick() => buttonClick.Play();
    public void PlayOpenWindow() => openWindow.Play();
    public void PlayCloseWindow() => closeWindow.Play();
    public void PlaySelectTile() => selectTile.Play();
    public void PlayDeselectTile() => deselectTile.Play();
    public void PlayPickupTile() => pickupTile.Play();
    public void PlayMoveTile() => moveTile.Play();
    public void PlayCancelMoveTile() => cancelMoveTile.Play();
    public void PlayPlaceTile() => placeTile.Play();
    public void PlayRotateTile() => rotateTile.Play();
    public void PlaySelectEquipment() => selectEquipment.Play();

    [Range(0f, 1f)]
    public float MasterVolume = 0.5f;

    [SerializeField]
    private List<AudioClipAndSettings> mainThemes;

    private readonly Dictionary<AudioType, float> audioTypeAndVolume = new Dictionary<AudioType, float>()
    {
        { AudioType.None, 1f },
        { AudioType.Effects, 1f },
        { AudioType.Music, 1f },
        { AudioType.Dialogue, 1f },
        { AudioType.UI, 1f },
    };

    public void RegisterExternalSource(AudioSource source, AudioClipAndSettings settings)
    => audioSourceContainer.Add(source, settings, settings.Type, AudioSourceLifecycle.PersistentExternal);

    public void UnregisterExternalSource(AudioSource source)
        => audioSourceContainer.RemoveSource(source);

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        foreach (AudioType type in Enum.GetValues(typeof(AudioType)))
            audioSourceContainer.ApplyVolumeToType(type, GetTotalAudioTypeVolume(type));
    }

    public void ChangeAudioTypeVolume(AudioType audioType, float volume)
    {
        audioTypeAndVolume[audioType] = Mathf.Clamp01(volume);
        audioSourceContainer.ApplyVolumeToType(audioType, GetTotalAudioTypeVolume(audioType));
    }

    public IReadOnlyDictionary<AudioType, float> AudioTypeVolume => audioTypeAndVolume;

    public float GetTotalAudioTypeVolume(AudioType audioType)
    {
        return audioTypeAndVolume.TryGetValue(audioType, out float v) ? v * MasterVolume : MasterVolume;
    }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < initialPoolSize; i++)
            pooledSources.Enqueue(CreatePooledSource());
    }

    private AudioSource CreatePooledSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    private void Start()
    {
        if (mainThemes.Count > 0)
            StartCoroutine(CycleMainThemes());
    }

    private bool mainThemeRegistered;

    private IEnumerator CycleMainThemes()
    {
        int currentIndex = 0;
        while (true)
        {
            if (mainMusicThemeSource.clip == null || (mainMusicThemeSource.time >= mainMusicThemeSource.clip.length && !mainMusicThemeSource.isPlaying))
            {
                AudioClipAndSettings audioClipAndSettings = new AudioClipAndSettings(mainThemes[currentIndex]) { loop = false };
                SetUpAudioSource(mainMusicThemeSource, audioClipAndSettings);

                if (!mainThemeRegistered)
                {
                    audioSourceContainer.Add(mainMusicThemeSource, audioClipAndSettings, AudioType.Music, AudioSourceLifecycle.PersistentExternal);
                    mainThemeRegistered = true;
                }
                else
                {
                    audioSourceContainer.UpdateSettings(mainMusicThemeSource, audioClipAndSettings);
                }

                mainMusicThemeSource.Play();
                currentIndex = (currentIndex + 1) % mainThemes.Count;
            }
            yield return null;
        }
    }

    public AudioSource PlayClipOnObject(AudioClipAndSettings clip, GameObject obj)
    {
        if (clip?.clip == null)
        {
            Debug.LogWarning("AudioManager: tried to play a clip with no AudioClip assigned.");
            return null;
        }

        AudioSource source = obj.AddComponent<AudioSource>();
        SetUpAudioSource(source, clip);
        audioSourceContainer.Add(source, clip, clip.Type, AudioSourceLifecycle.TransientExternal);
        source.Play();

        if (!clip.loop)
            StartCoroutine(WaitForEndOfAudio(source, AudioSourceLifecycle.TransientExternal));

        return source;
    }

    public AudioSource PlayClip(AudioClipAndSettings clip) => PlayClip(clip, null);

    public AudioSource PlayClip(AudioClipAndSettings clip, float? overrideVolume)
    {
        if (clip?.clip == null)
        {
            Debug.LogWarning("AudioManager: tried to play a clip with no AudioClip assigned.");
            return null;
        }

        AudioSource source = RentPooledSource();
        SetUpAudioSource(source, clip, overrideVolume);
        audioSourceContainer.Add(source, clip, clip.Type, AudioSourceLifecycle.Pooled);
        source.Play();

        if (!clip.loop)
            StartCoroutine(WaitForEndOfAudio(source, AudioSourceLifecycle.Pooled));

        return source;
    }

    private AudioSource RentPooledSource()
    {
        return pooledSources.Count > 0 ? pooledSources.Dequeue() : CreatePooledSource();
    }

    private void ReturnPooledSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        pooledSources.Enqueue(source);
    }

    private void SetUpAudioSource(AudioSource source, AudioClipAndSettings settings, float? overrideVolume = null)
    {
        source.clip = settings.clip;
        source.loop = settings.loop;
        source.volume = overrideVolume ?? settings.volume;
        source.pitch = settings.pitch == 0 ? 1 : settings.pitch;
        source.spatialBlend = settings.spatialBlend;
    }

    public void StopLoopingAudioClipOnceFinished(AudioSource source)
    {
        AudioSourceLifecycle lifecycle = audioSourceContainer.GetLifecycle(source);
        source.loop = false;
        StartCoroutine(WaitForEndOfAudio(source, lifecycle));
    }

    /// <summary>Stops and cleans up every source of a given type (e.g. mute all SFX on pause).</summary>
    public void StopAll(AudioType audioType)
    {
        foreach (AudioSource source in audioSourceContainer.GetSourcesOfType(audioType).ToList())
        {
            if (source == null) continue;
            source.Stop();

            AudioSourceLifecycle lifecycle = audioSourceContainer.GetLifecycle(source);
            if (lifecycle == AudioSourceLifecycle.PersistentExternal)
                continue; // owned by the scene object — just stop it, don't touch its lifetime

            audioSourceContainer.RemoveSource(source);
            if (lifecycle == AudioSourceLifecycle.TransientExternal) Destroy(source);
            else ReturnPooledSource(source);
        }
    }

    /// <summary>Stops and cleans up every managed source.</summary>
    public void StopAll()
    {
        foreach (AudioType type in Enum.GetValues(typeof(AudioType)))
            StopAll((AudioType)type);
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
                if (!audioSourceContainer.IsRegistered(source))
                    break;

                AudioSourceLifecycle lifecycle = audioSourceContainer.GetLifecycle(source);

                if (lifecycle == AudioSourceLifecycle.PersistentExternal)
                    break; // stays registered, owner manages it — just goes quiet

                audioSourceContainer.RemoveSource(source);
                if (lifecycle == AudioSourceLifecycle.TransientExternal)
                    Destroy(source);
                else
                    ReturnPooledSource(source);

                break;
            }

            yield return null;
        }
    }

    private IEnumerator WaitForEndOfAudio(AudioSource source, AudioSourceLifecycle lifecycle)
    {
        while (source != null && source.isPlaying)
            yield return null;

        if (source == null || !audioSourceContainer.IsRegistered(source))
            yield break;

        if (lifecycle == AudioSourceLifecycle.PersistentExternal)
            yield break; // owner manages this source's lifetime — just let it sit stopped, stay registered

        audioSourceContainer.RemoveSource(source);
        if (lifecycle == AudioSourceLifecycle.TransientExternal)
            Destroy(source);
        else
            ReturnPooledSource(source);
    }
}
