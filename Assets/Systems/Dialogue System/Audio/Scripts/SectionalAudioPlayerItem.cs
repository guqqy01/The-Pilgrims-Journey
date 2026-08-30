using UnityEngine;
using UnityEngine.Audio;

namespace DialogueSystem
{
    public class SectionalAudioPlayerItem
    {
        public readonly AudioMixerGroup AudioMixerGroup;
        public readonly AudioClip AudioClip;

        public readonly float setupPlaybackPosition;
        public readonly float actualPlaybackPosition;

        public readonly float startTime;
        public readonly float endTime;

        public readonly float timelineStartTime;
        public readonly float timelineEndTime;

        public SectionalAudioPlayerItem(AudioMixerGroup audioMixerGroup, AudioClip audioClip, float setupPlaybackPosition, float playbackPosition, float timeStart, float timeEnd, float timelineStartTime, float timelineEndTime)
        {
            AudioMixerGroup = audioMixerGroup;
            AudioClip = audioClip;
            this.setupPlaybackPosition = setupPlaybackPosition;
            actualPlaybackPosition = playbackPosition;
            startTime = timeStart;
            endTime = timeEnd;
            this.timelineStartTime = timelineStartTime;
            this.timelineEndTime = timelineEndTime;
        }

        public void SchedulePlay(AudioSource audioSource, double startTime)
        {
            audioSource.playOnAwake = false;
            audioSource.clip = AudioClip;
            audioSource.outputAudioMixerGroup = AudioMixerGroup;

            audioSource.time = actualPlaybackPosition;

            audioSource.PlayScheduled(startTime + this.startTime);
            audioSource.SetScheduledEndTime(startTime + endTime);
        }


    }
}

