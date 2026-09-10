using UnityEngine;

public class AudioSourceAndSettings
{
    public readonly AudioSource AudioSource;
    public AudioClipAndSettings ClipAndSettings { private set; get; }
    public readonly AudioSourceLifecycle Lifecycle;

    public AudioSourceAndSettings(AudioSource audioSource, AudioClipAndSettings clipAndSettings, AudioSourceLifecycle lifecycle)
    {
        AudioSource = audioSource;
        ClipAndSettings = clipAndSettings;
        Lifecycle = lifecycle;
    }

    public void UpdateAudioClipAndSettings(AudioClipAndSettings settings) => ClipAndSettings = settings;
}
