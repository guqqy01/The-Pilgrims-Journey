using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ManagedAudioSource : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    public AudioClipAndSettings audioClipSettings;

    public AudioSource Source => audioSource;
    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    private bool isRegistered;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void Play()
    {
        if (audioClipSettings?.clip == null)
        {
            Debug.LogWarning($"ManagedAudioSource on '{name}' has no clip assigned.", this);
            return;
        }

        audioSource.Play(audioClipSettings);

        if (!isRegistered)
        {
            AudioManager.Instance.RegisterExternalSource(audioSource, audioClipSettings);
            isRegistered = true;
        }
    }

    /// <summary>
    /// Plays the assigned clip starting partway through — useful for handing a clip
    /// off between sources without restarting it from 0:00 (e.g. AudioResiduleManager
    /// taking over a source that's about to be destroyed).
    /// </summary>
    public void Play(float startTime)
    {
        Play();
        if (audioSource.isPlaying && audioSource.clip != null && startTime > 0f && startTime < audioSource.clip.length)
            audioSource.time = startTime;
    }

    public void Stop()
    {
        audioSource.Stop();
        Unregister();
    }

    public void ApplyVolume(float multiplier = 1f)
    {
        multiplier = Mathf.Clamp01(multiplier);
        audioSource.volume = multiplier * audioClipSettings.volume * AudioManager.Instance.GetTotalAudioTypeVolume(audioClipSettings.Type);
    }

    private void OnDisable() => Unregister();
    private void OnDestroy() => Unregister();

    private void Unregister()
    {
        if (!isRegistered) return;
        AudioManager.Instance?.UnregisterExternalSource(audioSource);
        isRegistered = false;
    }
}