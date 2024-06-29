using System;
using UnityEngine;
using UnityEngine.Audio;

namespace DialogueSystem
{
    [Serializable]
    public class AudioDialogueMixerGroup : IComparable<AudioDialogueMixerGroup>, IDraggableRectTimeItem
    {
        public float Time;
        public AudioMixerGroup MixerGroup;

        public IDraggableRectTimeItem.TimePosition Position => IDraggableRectTimeItem.TimePosition.Center;

        public DraggableRectAxis Axis => DraggableRectAxis.Horizontal;

        float IDraggableRectTimeItem.StartTime { get => Time; set => Time = value; }

        public int CompareTo(AudioDialogueMixerGroup other)
        {
            return Time.CompareTo(other.Time);
        }

        public int CompareTo(IDraggableRectTimeItem other)
        {
            return Time.CompareTo(other.StartTime);
        }
    }
}