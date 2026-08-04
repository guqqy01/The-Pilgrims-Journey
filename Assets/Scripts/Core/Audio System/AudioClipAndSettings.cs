using System;
using UnityEngine;

[Serializable]
public class AudioClipAndSettings
{
    public AudioClip clip;
    public bool loop = false;
    [Range(0, 1)]
    public float volume = 1f;
    [Range(-3, 3)]
    public float pitch = 1f;

    [Range(0, 1)]
    public float spatialBlend = 1f;

    public AudioType Type;

    public AudioClipAndSettings(AudioClipAndSettings audioClipAndSettings)
    {
        clip = audioClipAndSettings.clip;
        loop = audioClipAndSettings.loop;
        volume = audioClipAndSettings.volume;
        pitch = audioClipAndSettings.pitch;
        spatialBlend = audioClipAndSettings.spatialBlend;
        Type = audioClipAndSettings.Type;
    }

    public void Play()
    {
        AudioManager.Instance.PlayClip(this, volume * AudioManager.Instance.GetTotalAudioTypeVolume(Type));
    }
}