using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class AudioExtensions
{
    public static void Setup(this AudioSource source, AudioClipAndSettings settings, AudioType? overrideType = null, float? overrideVolume = null)
    {
        source.clip = settings.clip;
        source.loop = settings.loop;

        float volume = overrideVolume ?? 
            settings.volume * (AudioManager.HasInstance ? AudioManager.Instance.GetTotalAudioTypeVolume(overrideType ?? settings.Type) : 1f);
        
        source.volume = volume;
        source.pitch = settings.pitch == 0 ? 1 : settings.pitch + UnityEngine.Random.Range(-settings.pitchVarianceRange / 2f, settings.pitchVarianceRange / 2f);
        source.spatialBlend = settings.spatialBlend;
    }

    public static void Play(this AudioSource source, AudioClipAndSettings settings, AudioType? overrideType = null, float? overrideVolume = null)
    {
        source.Setup(settings, overrideType, overrideVolume);
        source.Play();
    }
}