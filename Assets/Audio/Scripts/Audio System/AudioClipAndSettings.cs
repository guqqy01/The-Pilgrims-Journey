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
    public float pitchVarianceRange;
    [Range(0, 1)]
    public float spatialBlend = 1f;

    public AudioType Type;

    public AudioClipAndSettings(AudioClipAndSettings other)
    {
        clip = other.clip;
        loop = other.loop;
        volume = other.volume;
        pitch = other.pitch;
        pitchVarianceRange = other.pitchVarianceRange;
        spatialBlend = other.spatialBlend;
        Type = other.Type;
    }

    public void Play()
    {
        AudioManager.Instance.PlayClip(this);
    }
}