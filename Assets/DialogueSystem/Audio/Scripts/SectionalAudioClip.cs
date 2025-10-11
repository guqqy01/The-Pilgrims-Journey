using System;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;


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

        public float AudioClipStartTime => audioClip.length * slider.MinValue;
        public float AudioClipEndTime => audioClip.length * slider.MaxValue;
        public float EndTime => StartTime + slider.ValueSpan * audioClip.length;

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
                return slider.ValueSpan * audioClip.length;
            }
            set
            {
                slider.ValueSpan = value / audioClip.length;
            }
        }

        public float MinValue
        {
            get
            {
                return slider.MinValue;
            }
            set 
            {
                slider.MinValue = value;
            }
        }
        public float MaxValue
        {
            get
            {
                return slider.MaxValue;
            }
            set
            {
                slider.MaxValue = value;
            }
        }

        public float MinTime
        {
            get
            {
                return slider.MinValue * audioClip.length;
            }
            set
            {
                slider.MinValue = Mathf.Clamp01(value / audioClip.length);
            }
        }


        public float MaxTime
        {
            get
            {
                return slider.MaxValue * audioClip.length;
            }
            set
            {
                slider.MaxValue = Mathf.Clamp01(value / audioClip.length);
            }
        }


        public IDraggableRectTimeItem.TimePosition Position { get { return IDraggableRectTimeItem.TimePosition.Start; } }
        public DraggableRectAxis Axis { get { return DraggableRectAxis.Horizontal; } }

        public SectionalAudioClip(AudioClip audioClip, float startT, float endT)
        {
            this.audioClip = audioClip;
            slider = new FloatRangeSlider();
            slider.MinValue = startT;
            slider.MaxValue = endT;
        }

        public SectionalAudioClip(AudioClip audioClip, float startT, float endT, float time)
        {
            this.audioClip = audioClip;
            slider = new FloatRangeSlider();
            slider.MinValue = startT;
            slider.MaxValue = endT;

            this.time = time;
        }

        public SectionalAudioClip[] Split(float T)
        {
            SectionalAudioClip[] twoSectionalAudioClips = new SectionalAudioClip[2];

            twoSectionalAudioClips[0] = new SectionalAudioClip(audioClip, slider.MinValue, Mathf.Lerp(slider.MinValue, slider.MaxValue, T), time);
            twoSectionalAudioClips[1] = new SectionalAudioClip(audioClip, Mathf.Lerp(slider.MinValue, slider.MaxValue, T), slider.MaxValue, time + (Mathf.Lerp(slider.MinValue, slider.MaxValue, T) - slider.MinValue) * audioClip.length);

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
