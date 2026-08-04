using System.Collections.Generic;
using UnityEngine;

public readonly struct WaveData
{
    public readonly WaveDataLOD Lod;

    public readonly List<float> averagePositions;
    public readonly List<float> lowRMSquaredList;
    public readonly List<float> highRMSquaredList;

    public WaveData(AudioClip clip, WaveDataLOD Lod)
    {
        
        this.Lod = Lod;
        averagePositions = new List<float>();
        lowRMSquaredList = new List<float>();
        highRMSquaredList = new List<float>();

        if (clip == null)
            return;

        float[] data = new float[clip.samples * clip.channels];

        if (clip.GetData(data, 0))
        {
            int amountOfLines = data.Length * (int)Lod / 2000;

            int skipLength = Mathf.RoundToInt(data.Length / amountOfLines);
            int startIndex = 0;

            for (int i = 0; i < amountOfLines; i++)
            {
                float average = 0;
                float high = 0;
                float low = 0;
                int highCount = 0;
                int lowCount = 0;
                int currentIndex;

                for (currentIndex = startIndex; currentIndex < Mathf.Min(startIndex + skipLength, data.Length); currentIndex++)
                {
                    average += data[currentIndex];
                }

                average /= skipLength;

                for (currentIndex = startIndex; currentIndex < Mathf.Min(startIndex + skipLength, data.Length); currentIndex++)
                {
                    float value = data[currentIndex];
                    if (value < 0)
                    {
                        low += value * value;
                        lowCount++;
                    }
                    else
                    {
                        high += value * value;
                        highCount++;
                    }
                }

                high = Mathf.Sqrt(high / highCount);
                low = Mathf.Sqrt(low / lowCount);

                startIndex = currentIndex;

                averagePositions.Add(average);
                highRMSquaredList.Add(high);
                lowRMSquaredList.Add(low);
            }
        }
    }

    public WaveData(AudioClip clip, int Lod)
    {
        switch (Lod)
        {
            case (int)WaveDataLOD.Low:
                this.Lod = WaveDataLOD.Low;
                break;
            case (int)WaveDataLOD.Medium:
                this.Lod = WaveDataLOD.Medium;
                break;
            case (int)WaveDataLOD.High:
                this.Lod = WaveDataLOD.High;
                break;
            default:
                this.Lod = WaveDataLOD.Custom;
                break;
        }

        
        averagePositions = new List<float>();
        lowRMSquaredList = new List<float>();
        highRMSquaredList = new List<float>();

        if (clip == null)
            return;

        float[] data = new float[clip.samples * clip.channels];

        if (clip.GetData(data, 0))
        {
            int amountOfLines = data.Length * (int)Lod / 2000;

            int skipLength = Mathf.RoundToInt(data.Length / amountOfLines);
            int startIndex = 0;

            for (int i = 0; i < amountOfLines; i++)
            {
                float average = 0;
                float high = 0;
                float low = 0;
                int highCount = 0;
                int lowCount = 0;
                int currentIndex;

                for (currentIndex = startIndex; currentIndex < Mathf.Min(startIndex + skipLength, data.Length); currentIndex++)
                {
                    average += data[currentIndex];
                }

                average /= skipLength;

                for (currentIndex = startIndex; currentIndex < Mathf.Min(startIndex + skipLength, data.Length); currentIndex++)
                {
                    float value = data[currentIndex];
                    if (value < 0)
                    {
                        low += value * value;
                        lowCount++;
                    }
                    else
                    {
                        high += value * value;
                        highCount++;
                    }
                }

                high = Mathf.Sqrt(high / highCount);
                low = Mathf.Sqrt(low / lowCount);

                startIndex = currentIndex;

                averagePositions.Add(average);
                highRMSquaredList.Add(high);
                lowRMSquaredList.Add(low);
            }
        }
    }

}