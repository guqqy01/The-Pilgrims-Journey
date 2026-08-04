using DialogueSystem;
using UnityEngine;
public class SectionalAudioClipViewer
{
    public readonly SectionalAudioClip sectionalAudioClip;

    public AudioClip AudioClip 
    {
        get
        {
            return sectionalAudioClip.audioClip;
        }
        set
        {
            if (sectionalAudioClip.audioClip != value)
            {
                sectionalAudioClip.audioClip = value;
                AudioClipWaveData = new AudioClipWaveData(AudioClip, LOD);
            }
        }
    }
    

    public DrawMethod drawMethod = DrawMethod.Cutout;

    public AudioClipWaveData AudioClipWaveData;

    public int LOD;

    public WaveData WaveData => AudioClipWaveData[LOD];
    public WaveData GetWaveData(int lod) => AudioClipWaveData[LOD];
    public float MinValue => sectionalAudioClip.slider.Start;
    public float MaxValue => sectionalAudioClip.slider.End;
    public SectionalAudioClipViewer(SectionalAudioClip sectionalAudioClip, int lod, DrawMethod drawMethod)
    {
        this.sectionalAudioClip = sectionalAudioClip;

        LOD = lod;
        this.drawMethod = drawMethod;

        AudioClipWaveData = new AudioClipWaveData(AudioClip, Mathf.Max(lod, 100));
    }
}
