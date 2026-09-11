using System.Collections.Generic;
using UnityEngine;

public class AudioManagerSourcesContainer
{
    private readonly List<AudioSourceAndSettings> audioSources = new List<AudioSourceAndSettings>();
    private readonly Dictionary<AudioType, List<AudioSourceAndSettings>> audioTypeSources = new Dictionary<AudioType, List<AudioSourceAndSettings>>();

    public bool IsRegistered(AudioSource source) => TryFindTypeSourceStoredInDictionary(source, out _, out _);

    public void ApplyVolumeToType(AudioType audioType, float totalVolume)
    {
        if (!audioTypeSources.TryGetValue(audioType, out List<AudioSourceAndSettings> sources))
            return;

        foreach (AudioSourceAndSettings entry in sources)
        {
            if (entry.AudioSource == null) continue;
            entry.AudioSource.volume = entry.ClipAndSettings.volume * totalVolume;
        }
    }

    public void ApplyPitchToType(AudioType audioType, float totalPitch)
    {
        if (!audioTypeSources.TryGetValue(audioType, out List<AudioSourceAndSettings> sources))
            return;

        foreach (AudioSourceAndSettings entry in sources)
        {
            if (entry.AudioSource == null) continue;
            entry.AudioSource.pitch = entry.Pitch * totalPitch;
        }
    }

    public void UpdateSettings(AudioSource audioSource, AudioClipAndSettings settings)
    {
        if(TryFindTypeSourceStoredInDictionary(audioSource, out AudioSourceAndSettings returnSourceAndSetting, out AudioType returnAudioType))
        {
            returnSourceAndSetting.UpdateAudioClipAndSettings(settings);
        }
    }

    public IEnumerable<AudioSource> GetSourcesOfType(AudioType audioType)
    {
        if (!audioTypeSources.TryGetValue(audioType, out List<AudioSourceAndSettings> sources))
            yield break;

        foreach (AudioSourceAndSettings entry in sources)
            yield return entry.AudioSource;
    }

    public void Add(AudioSource source, AudioClipAndSettings settings, float pitch, AudioType audioType, AudioSourceLifecycle lifecycle)
    {
        var entry = new AudioSourceAndSettings(source, settings, pitch, lifecycle);
        audioSources.Add(entry);
        if (!audioTypeSources.TryGetValue(audioType, out var list))
        {
            list = new List<AudioSourceAndSettings>();
            audioTypeSources.Add(audioType, list);
        }
        list.Add(entry);
    }

    public AudioSourceLifecycle GetLifecycle(AudioSource source)
    {
        return TryFindTypeSourceStoredInDictionary(source, out var entry, out _)
            ? entry.Lifecycle
            : AudioSourceLifecycle.TransientExternal; // unknown — safest to destroy, not pool
    }

    public void RemoveSource(AudioSource source)
    {
        if (TryFindTypeSourceStoredInDictionary(source, out AudioSourceAndSettings returnSourceAndSetting, out AudioType returnAudioType))
        {
            RemoveSourceAndSettings(returnSourceAndSetting, returnAudioType);
        }
    }

    private void RemoveSourceAndSettings(AudioSourceAndSettings returnSourceAndSetting, AudioType returnAudioType)
    {
        audioSources.Remove(returnSourceAndSetting);

        if (audioTypeSources.TryGetValue(returnAudioType, out List<AudioSourceAndSettings> list))
        {
            list.Remove(returnSourceAndSetting);
        }
    }

    private bool TryFindTypeSourceStoredInDictionary(AudioSource source, out AudioSourceAndSettings returnSourceAndSetting, out AudioType returnAudioType)
    {
        foreach (var kvp in audioTypeSources)
        {
            foreach (AudioSourceAndSettings sourceAndSetting in kvp.Value)
            {
                if (source == sourceAndSetting.AudioSource)
                {
                    returnAudioType = kvp.Key;
                    returnSourceAndSetting = sourceAndSetting;
                    return true;
                }
            }
        }
        returnAudioType = AudioType.None;
        returnSourceAndSetting = null;
        return false;
    }
}
