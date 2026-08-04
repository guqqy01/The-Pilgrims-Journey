using System.Collections.Generic;
using UnityEngine;

public readonly struct AudioClipWaveData
{
    public readonly AudioClip clip;
    public readonly Dictionary<int, WaveData> LodAndWaveDatas;

    public WaveData this[int LOD]
    {
        get
        {
            return LodAndWaveDatas[LOD];
        }
    }

    public AudioClipWaveData(AudioClip clip)
    {
        LodAndWaveDatas = new Dictionary<int, WaveData>();
        this.clip = clip;

        LodAndWaveDatas.Add((int)WaveDataLOD.Low, new WaveData(clip, WaveDataLOD.Low));
        LodAndWaveDatas.Add((int)WaveDataLOD.Medium, new WaveData(clip, WaveDataLOD.Medium));
        LodAndWaveDatas.Add((int)WaveDataLOD.High, new WaveData(clip, WaveDataLOD.High));
    }

    public AudioClipWaveData(AudioClip clip, params int[] customLods)
    {
        LodAndWaveDatas = new Dictionary<int, WaveData>();
        this.clip = clip;

        LodAndWaveDatas.Add((int)WaveDataLOD.Low, new WaveData(clip, WaveDataLOD.Low));
        LodAndWaveDatas.Add((int)WaveDataLOD.Medium, new WaveData(clip, WaveDataLOD.Medium));
        LodAndWaveDatas.Add((int)WaveDataLOD.High, new WaveData(clip, WaveDataLOD.High));

        foreach (int lod in customLods)
        {
            if (LodAndWaveDatas.ContainsKey(lod))
                continue;

            LodAndWaveDatas.Add(lod, new WaveData(clip, lod));
        }
        
    }
}
