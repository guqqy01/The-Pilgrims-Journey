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

        audioSource.clip = audioClipSettings.clip;
        audioSource.loop = audioClipSettings.loop;
        audioSource.pitch = audioClipSettings.pitch == 0 ? 1 : audioClipSettings.pitch;
        ApplyVolume();

        audioSource.Play();

        if (!isRegistered)
        {
            AudioManager.Instance.RegisterExternalSource(audioSource, audioClipSettings);
            isRegistered = true;
        }
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
