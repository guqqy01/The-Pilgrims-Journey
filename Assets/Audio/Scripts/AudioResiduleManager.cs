using System.Collections.Generic;
using UnityEngine;

public class AudioResiduleManager : Singleton<AudioResiduleManager>
{
    [SerializeField]
    private AudioResidule audioResiduePrefab;

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 8;
    private readonly Queue<AudioResidule> pooledResidues = new Queue<AudioResidule>();
    private readonly List<AudioResidule> activeResidules = new List<AudioResidule>();

    public int ActiveCount => activeResidules.Count;

    private void Awake()
    {
        base.InitiateFromRootAtAwake();

        for (int i = 0; i < initialPoolSize; i++)
            pooledResidues.Enqueue(CreatePooledResidule());
    }

    private AudioResidule CreatePooledResidule()
    {
        AudioResidule residule = Instantiate(audioResiduePrefab, transform);
        residule.gameObject.SetActive(false);
        return residule;
    }

    /// <summary>
    /// Spawns a residule at target's position that follows it every frame, plays the
    /// given clip, and fades itself out automatically if target is destroyed before
    /// the clip finishes. This is the normal path — call it whenever a sound needs to
    /// keep playing independent of the lifetime of the thing that triggered it
    /// (e.g. attack spawn/impact audio).
    /// </summary>
    public void CreateResidule(AudioResiduleSettings settings, Transform target, float startTime = 0f)
    {
        if (settings?.clip?.clip == null)
        {
            Debug.LogWarning("AudioResiduleManager: tried to create a residule with no AudioClip assigned.");
            return;
        }

        AudioResidule residule = RentResidule();
        residule.transform.position = target != null ? target.position : Vector3.zero;
        residule.gameObject.SetActive(true);
        activeResidules.Add(residule);
        residule.Play(settings, target, startTime, ReturnResidule);
    }

    /// <summary>
    /// Rescue path for a sound already playing on a ManagedAudioSource that's about to
    /// be destroyed/disabled. Spawns a residule that resumes the same clip from where
    /// the source left off, then stops the original source. Prefer routing audio
    /// through CreateResidule from the start where you can predict the need up front —
    /// this is for existing ManagedAudioSource usages you haven't migrated yet.
    /// </summary>
    public void TakeOverFromManagedSource(ManagedAudioSource source, Transform target, float fadeTime)
    {
        if (source == null || !source.IsPlaying)
            return;

        var settings = new AudioResiduleSettings
        {
            clip = source.audioClipSettings,
            fadeTime = fadeTime
        };

        float resumeFrom = source.Source.time;
        CreateResidule(settings, target, resumeFrom);

        source.Stop();
    }

    /// <summary>Immediately cuts every active residule — useful on scene unload/teardown.</summary>
    public void StopAllResidules()
    {
        // Copy first: StopImmediately -> Finish -> ReturnResidule mutates activeResidules.
        var toStop = new List<AudioResidule>(activeResidules);
        foreach (var residule in toStop)
            residule.StopImmediately();
    }

    private AudioResidule RentResidule()
    {
        return pooledResidues.Count > 0 ? pooledResidues.Dequeue() : CreatePooledResidule();
    }

    private void ReturnResidule(AudioResidule residule)
    {
        activeResidules.Remove(residule);
        residule.gameObject.SetActive(false);
        pooledResidues.Enqueue(residule);
    }
}