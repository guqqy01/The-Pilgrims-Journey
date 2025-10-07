using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.Audio;

namespace DialogueSystem
{
    [Serializable]
    public class AudioDialogue
    {
        public List<AudioDialogueMixerGroup> audioDialogueMixerGroups = new List<AudioDialogueMixerGroup>();

        public List<SectionalAudioClip> sectionalAudioClips = new List<SectionalAudioClip>();
        
        public List<SectionalAudioPlayerItem> GetSetupForPlayer(float currentTimeOnPlayer)
        {
            List<SectionalAudioPlayerItem> returnList = new List<SectionalAudioPlayerItem>();

            List<IDraggableRectTimeItem> list = new List<IDraggableRectTimeItem>();

            foreach (AudioDialogueMixerGroup audioDialogueMixerGroup in audioDialogueMixerGroups)
            {
                list.Add(audioDialogueMixerGroup);
            }

            foreach (SectionalAudioClip sectionalAudioClip in sectionalAudioClips)
            {
                list.Add(sectionalAudioClip);
            }

            list.Sort();

            SectionalAudioClip currentSectionalAudioClip = null;
            AudioDialogueMixerGroup currentAudioDialogueMixerGroup = null;

            for (int i = 0; i < list.Count;)
            {
                float currentItemStartTime = list[i].StartTime;

                while (i < list.Count && list[i].StartTime == currentItemStartTime)
                {
                    if (list[i] is SectionalAudioClip sectionalAudioClip)
                    {
                        currentSectionalAudioClip = sectionalAudioClip;
                    }
                    else if (list[i] is AudioDialogueMixerGroup audioDialogueMixer)
                    {
                        currentAudioDialogueMixerGroup = audioDialogueMixer;
                    }

                    i++;
                }

                if (currentSectionalAudioClip == null || currentSectionalAudioClip.EndTime < currentItemStartTime)
                    continue;

                float EndTime = i < list.Count ? Mathf.Min(currentSectionalAudioClip.EndTime, list[i].StartTime) : currentSectionalAudioClip.EndTime;

                if (EndTime < currentTimeOnPlayer)
                    continue;



                SectionalAudioPlayerItem audioDialoguePlayerSectionalAudioItem = new SectionalAudioPlayerItem
                    (
                    audioMixerGroup: (currentAudioDialogueMixerGroup != null) ? currentAudioDialogueMixerGroup.MixerGroup : null,
                    audioClip: currentSectionalAudioClip.audioClip,
                    setupPlaybackPosition: currentSectionalAudioClip.GetSetupPlayBackPosition(currentItemStartTime),
                    playbackPosition: currentSectionalAudioClip.GetPlaybackPosition(currentItemStartTime, currentTimeOnPlayer),
                    timeStart: currentItemStartTime - currentTimeOnPlayer,
                    timeEnd: EndTime - currentTimeOnPlayer,
                    timelineStartTime: currentItemStartTime,
                    timelineEndTime: EndTime
                    );

                returnList.Add(audioDialoguePlayerSectionalAudioItem);
            }

            return returnList;
        }

        public void AddSectionAudioClip(AudioClip audioClip, float time, float startT, float endT)
        {
            SectionalAudioClip sectionalAudioClip = new SectionalAudioClip(audioClip, startT, endT, time);

            sectionalAudioClips.Add(sectionalAudioClip);
        }
    }
}