using UnityEngine;

public class AudioSourceAndSettings
{
    public readonly AudioSource AudioSource;
    public AudioClipAndSettings ClipAndSettings { private set; get; }
    public float Pitch;
    public readonly AudioSourceLifecycle Lifecycle;

    public AudioSourceAndSettings(AudioSource audioSource, AudioClipAndSettings clipAndSettings, float pitch, AudioSourceLifecycle lifecycle)
    {
        AudioSource = audioSource;
        ClipAndSettings = clipAndSettings;
        Pitch = pitch;
        Lifecycle = lifecycle;
    }

    public void UpdateAudioClipAndSettings(AudioClipAndSettings settings) => ClipAndSettings = settings;
}
