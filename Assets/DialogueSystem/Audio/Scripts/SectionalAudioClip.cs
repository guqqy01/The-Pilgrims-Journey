using System;
using UnityEngine;


namespace DialogueSystem
{
    [Serializable]
    public class SectionalAudioClip : IListOverTimeItem, IDraggableRectTimeLengthItem, IComparable<SectionalAudioClip>
    {
        public AudioClip audioClip;

        public FloatRangeSlider slider;

        [SerializeField]
        private float time;

        public float StartTime
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
            }
        }

        public float AudioClipStartTime => audioClip.length * slider.Start;
        public float AudioClipEndTime => audioClip.length * slider.End;
        public float EndTime => StartTime + slider.Length * audioClip.length;

        public float GetSetupPlayBackPosition(float currentStartTim)
        {
            float relativeTime = currentStartTim - StartTime;

            return Mathf.Clamp(AudioClipStartTime + relativeTime, AudioClipStartTime, AudioClipEndTime);
        }

        public float GetPlaybackPosition(float currentStartTime, float currentTimeOnPlayer)
        {
            float maxTime = Mathf.Max(currentStartTime, currentTimeOnPlayer);

            float relativeTime = maxTime - StartTime;

            return Mathf.Clamp(AudioClipStartTime + relativeTime, AudioClipStartTime, AudioClipEndTime);
        }

        public float Length
        {
            get
            {
                return slider.Length * audioClip.length;
            }
            set
            {
                slider.Length = value / audioClip.length;
            }
        }

        public float MinValue
        {
            get
            {
                return slider.Start;
            }
            set 
            {
                slider.Start = value;
            }
        }
        public float MaxValue
        {
            get
            {
                return slider.End;
            }
            set
            {
                slider.End = value;
            }
        }

        public float MinTime
        {
            get
            {
                return slider.Start * audioClip.length;
            }
            set
            {
                slider.Start = Mathf.Clamp01(value / audioClip.length);
            }
        }


        public float MaxTime
        {
            get
            {
                return slider.End * audioClip.length;
            }
            set
            {
                slider.End = Mathf.Clamp01(value / audioClip.length);
            }
        }


        public IDraggableRectTimeItem.TimePosition Position { get { return IDraggableRectTimeItem.TimePosition.Start; } }
        public DraggableRectAxis Axis { get { return DraggableRectAxis.Horizontal; } }

        public SectionalAudioClip(AudioClip audioClip, float startT, float endT)
        {
            this.audioClip = audioClip;
            slider = new FloatRangeSlider();
            slider.Start = startT;
            slider.End = endT;
        }

        public SectionalAudioClip(AudioClip audioClip, float startT, float endT, float time)
        {
            this.audioClip = audioClip;
            slider = new FloatRangeSlider();
            slider.Start = startT;
            slider.End = endT;

            this.time = time;
        }

        public SectionalAudioClip[] Split(float T)
        {
            SectionalAudioClip[] twoSectionalAudioClips = new SectionalAudioClip[2];

            twoSectionalAudioClips[0] = new SectionalAudioClip(audioClip, slider.Start, Mathf.Lerp(slider.Start, slider.End, T), time);
            twoSectionalAudioClips[1] = new SectionalAudioClip(audioClip, Mathf.Lerp(slider.Start, slider.End, T), slider.End, time + (Mathf.Lerp(slider.Start, slider.End, T) - slider.Start) * audioClip.length);

            return twoSectionalAudioClips;
        }

        public int CompareTo(IListOverTimeItem other)
        {
            return StartTime.CompareTo(other.StartTime);
        }

        public int CompareTo(SectionalAudioClip other)
        {
            return StartTime.CompareTo(other.StartTime);
        }

        public int CompareTo(IDraggableRectTimeItem other)
        {
            return StartTime.CompareTo(other.StartTime);
        }
    }

}
