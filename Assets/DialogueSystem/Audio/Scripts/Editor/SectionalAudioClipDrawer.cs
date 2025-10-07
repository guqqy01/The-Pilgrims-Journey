using DialogueSystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SectionalAudioClipDrawer
{
    public static void DrawAudioClip(Rect rect, WaveData data, Vector2 sliderPosition, float minValue, float maxValue)
    {
        List<float> averagePositions = new List<float>(data.averagePositions);
        List<float> lowRMSquaredList = new List<float>(data.lowRMSquaredList);
        List<float> highRMSquaredList = new List<float>(data.highRMSquaredList);

        int minIndex = Mathf.FloorToInt((sliderPosition.Min() * (maxValue - minValue) + minValue) * averagePositions.Count);
        int maxIndex = Mathf.CeilToInt((sliderPosition.Max() * (maxValue - minValue) + minValue) * averagePositions.Count);

        averagePositions = averagePositions.GetRange(minIndex, maxIndex - minIndex);
        lowRMSquaredList = lowRMSquaredList.GetRange(minIndex, maxIndex - minIndex);
        highRMSquaredList = highRMSquaredList.GetRange(minIndex, maxIndex - minIndex);

        Handles.color = Color.yellow;

        for (int i = 0; i < averagePositions.Count - 1; i++)
        {
            float center = (rect.yMax - rect.center.y) * averagePositions[i] + rect.center.y;

            float yMin1 = center - lowRMSquaredList[i] * rect.height - 0.01f * rect.height;
            float yMax1 = center + highRMSquaredList[i] * rect.height + 0.01f * rect.height;

            float xPos1 = rect.xMin + rect.width * ((float)(i) / averagePositions.Count);

            float yMin2 = center - lowRMSquaredList[i + 1] * rect.height - 0.01f * rect.height;
            float yMax2 = center + highRMSquaredList[i + 1] * rect.height + 0.01f * rect.height;

            float xPos2 = rect.xMin + rect.width * ((float)(i + 1) / averagePositions.Count);

            Handles.DrawAAConvexPolygon(new Vector2(xPos1, yMin1), new Vector2(xPos1, yMax1), new Vector2(xPos2 + 1, yMax2), new Vector2(xPos2 + 1, yMin2));
        }
    }

    public static void DrawAudioClip(Rect rect, WaveData data, DrawMethod drawMethod, float minValue = 0, float maxValue = 1)
    {
        List<float> averagePositions = new List<float>(data.averagePositions);
        List<float> lowRMSquaredList = new List<float>(data.lowRMSquaredList);
        List<float> highRMSquaredList = new List<float>(data.highRMSquaredList);

        if (drawMethod == DrawMethod.Cutout)
        {
            int minIndex = Mathf.FloorToInt(minValue * averagePositions.Count);
            int maxIndex = Mathf.CeilToInt(maxValue * averagePositions.Count);

            averagePositions = averagePositions.GetRange(minIndex, maxIndex - minIndex);
            lowRMSquaredList = lowRMSquaredList.GetRange(minIndex, maxIndex - minIndex);
            highRMSquaredList = highRMSquaredList.GetRange(minIndex, maxIndex - minIndex);
        }

        Handles.color = Color.yellow;

        for (int i = 0; i < averagePositions.Count - 1; i++)
        {
            float center = (rect.yMax - rect.center.y) * averagePositions[i] + rect.center.y;

            float yMin1 = center - lowRMSquaredList[i] * rect.height - 0.01f * rect.height;
            float yMax1 = center + highRMSquaredList[i] * rect.height + 0.01f * rect.height;

            float xPos1 = rect.xMin + rect.width * ((float)(i) / averagePositions.Count);

            float yMin2 = center - lowRMSquaredList[i + 1] * rect.height - 0.01f * rect.height;
            float yMax2 = center + highRMSquaredList[i + 1] * rect.height + 0.01f * rect.height;

            float xPos2 = rect.xMin + rect.width * ((float)(i + 1) / averagePositions.Count);

            Handles.color = (i < minValue * averagePositions.Count || i > maxValue * averagePositions.Count) && drawMethod == DrawMethod.Fade ? (Color.yellow * 0.2f).SetAlpha(1f) : Color.yellow;
            Handles.DrawAAConvexPolygon(new Vector2(xPos1, yMin1), new Vector2(xPos1, yMax1), new Vector2(xPos2 + 1, yMax2), new Vector2(xPos2 + 1, yMin2));
        }
    }

    public static void DrawAudioClip(Rect rect, SectionalAudioClipViewer audioClipViewer)
    {
        DrawAudioClip(rect, audioClipViewer.WaveData, DrawMethod.Cutout, audioClipViewer.MinValue, audioClipViewer.MaxValue);
    }
}
