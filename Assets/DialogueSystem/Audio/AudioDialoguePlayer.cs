using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace DialogueSystem
{
    public class AudioDialoguePlayer : MonoBehaviour
    {
        private AudioDialogue currentlyPlayingAudioDialogue;

        private readonly List<AudioSource> audioSources = new List<AudioSource>();
        private readonly List<SectionalAudioPlayerItem> audioDialoguePlayerSectionalAudioItems = new List<SectionalAudioPlayerItem>();

        private readonly Dictionary<AudioSource, SectionalAudioPlayerItem> audioSourcesAndAudioItems = new Dictionary<AudioSource, SectionalAudioPlayerItem>();

        /// <summary>
        /// The current playback time.
        /// </summary>
        private float currentTime;

        private double relativeStartTimeDPS;

        private float endTime;

        public PlayerState PlayerState { get; private set; }

        public bool IsPlaying => PlayerState == PlayerState.Playing;
        public bool IsPaused => PlayerState == PlayerState.Paused;
        public bool IsStopped => PlayerState == PlayerState.Stopped;

        private bool loop;
        public bool Loop
        {
            get
            {
                return loop;
            }
            set
            {
                loop = value;
            }
        }

        /// <summary>
        /// The current playback time in the audioDialogue timeline
        /// </summary>
        public float CurrentTime
        {
            get
            {
                if(IsPlaying)
                {
                    CheckCurrentTimeWhilePlaying();
                }
                else if(IsStopped && currentTime != 0)
                {
                    currentTime = 0;
                }

                return currentTime;
            }
            set
            {
                currentTime = value;

                Pause();
            }
        }


        private void CheckCurrentTimeWhilePlaying()
        {
            float relativeCurrentTime = (float)(AudioSettings.dspTime - relativeStartTimeDPS);

            float endTime = 0f;

            for (int i = 0; i < audioSources.Count; i++)
            {
                AudioSource audioSource = audioSources[i];

                if (!audioSourcesAndAudioItems.ContainsKey(audioSource))
                    continue;

                SectionalAudioPlayerItem audioItem = audioSourcesAndAudioItems[audioSource];

                if(audioItem.timelineEndTime > endTime)
                    endTime = audioItem.timelineEndTime;

                if (relativeCurrentTime.IsWithin(audioItem.timelineStartTime, audioItem.timelineEndTime))
                {
                    float currentTime = audioItem.timelineStartTime - audioItem.setupPlaybackPosition + audioSource.time;

                    if (this.currentTime != currentTime)
                    {
                        this.currentTime = currentTime;
                        relativeStartTimeDPS = AudioSettings.dspTime - currentTime;
                    }

                    return;
                }
            }

            if(currentTime >= endTime)
            {
                Stop();

                if (Loop)
                {
                    Play(currentlyPlayingAudioDialogue);
                }
            }
            else
            {
                currentTime = relativeCurrentTime;
            }

        }

        public void Play(AudioDialogue setupAudioDialogue)
        {
            CurrentTime = 0;

            if (setupAudioDialogue == currentlyPlayingAudioDialogue && PlayerState == PlayerState.Playing)
                return;

            currentlyPlayingAudioDialogue = setupAudioDialogue;

            relativeStartTimeDPS = AudioSettings.dspTime - CurrentTime;

            Setup(setupAudioDialogue, CurrentTime);

            foreach (KeyValuePair<AudioSource, SectionalAudioPlayerItem> kvp in audioSourcesAndAudioItems)
            {
                AudioSource audioSource = kvp.Key;
                SectionalAudioPlayerItem audioPlayerItem = kvp.Value;

                audioPlayerItem.SchedulePlay(audioSource, AudioSettings.dspTime);
            }
            
            PlayerState = PlayerState.Playing;

            if(Application.isPlaying)
                StartCoroutine(ManagePlayState(setupAudioDialogue));
        }


        public IEnumerator ManagePlayState(AudioDialogue audioDialogue)
        {
            while (IsPlaying)
            {
                CheckCurrentTimeWhilePlaying();

                bool playing = false;
                foreach (AudioSource source in audioSources)
                {
                    if (source.isPlaying)
                    {
                        playing = true;
                        break;
                    }
                }

                if (!playing)
                {
                    if (loop)
                    {
                        CurrentTime = 0;
                        Play(audioDialogue);
                        break;
                    }
                    else
                    {
                        Stop();
                    }
                }

                yield return null;
            }
        }


        private void Setup(AudioDialogue audioDialogue, float time = 0)
        {
            ClearForSetup();

            List<SectionalAudioPlayerItem> audioDialoguePlayerSectionalAudioItems = audioDialogue.GetSetupForPlayer(time);

            int initialAudioSourceCount = audioSources.Count;
            int audioSourceIndex = 0;

            for (int i = 0; i < audioDialoguePlayerSectionalAudioItems.Count; i++)
            {
                AudioSource audioSource;

                if (audioSourceIndex < initialAudioSourceCount)
                {
                    audioSource = audioSources[audioSourceIndex];
                    audioSourceIndex++;
                }
                else
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSources.Add(audioSource);
                }

                audioSourcesAndAudioItems.Add(audioSource, audioDialoguePlayerSectionalAudioItems[i]);
            }
        }

        private void ClearForSetup()
        {
            audioSourcesAndAudioItems.Clear();
            audioSources.RemoveDuplicates();

            foreach (AudioSource audioSource in audioSources)
            {
                audioSource.Stop();
            }
        }

        public void Pause()
        {
            foreach (AudioSource source in audioSources)
            {
                source.Pause();

                source.SetScheduledEndTime(double.MaxValue);
            }

            PlayerState = PlayerState.Paused;
        }

        public void Stop()
        {
            foreach (AudioSource source in audioSources)
            {
                source.Stop();
            }

            PlayerState = PlayerState.Stopped;
        }
    }
}

