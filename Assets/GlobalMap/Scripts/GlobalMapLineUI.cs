using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GlobalMapLineUI : MonoBehaviour
{
    public LineRenderer LineRenderer;

    [SerializeField]
    private bool play;
    private float timeToStart;

    private float colorChangeSpeed;
    private float t;
    [SerializeField]
    private float gap;

    private Color endOfLineColor;
    private Color frontOfLineColor;

    private float currentXOffset;
    [SerializeField]
    private float offsetXSpeed;

    private Material lineMaterial;

    private void Awake()
    {
        lineMaterial = LineRenderer.material;
    }

    public void SetupPosition(Vector2 fromPos, Vector2 toPos)
    {
        LineRenderer.positionCount = 2;
        Vector3[] points = new Vector3[2] { fromPos, toPos };
        LineRenderer.SetPositions(points);
    }

    private void Update()
    {
        currentXOffset -= offsetXSpeed * Time.deltaTime;
        lineMaterial.mainTextureOffset = new Vector2(currentXOffset, 1);

        if (play && Time.time >= timeToStart)
        {
            t = Mathf.Clamp01(t + colorChangeSpeed * Time.deltaTime);

            SetColorGradient(t);

            if ((colorChangeSpeed.Positive() && t >= 1) || (colorChangeSpeed.Negative() && t <= 0) || colorChangeSpeed == 0)
            {
                play = false;
            }
        }
    }
    
    public void PlayColorChange(Color frontOfLineColor, Color endOfLineColor,  float timeToStart, float colorChangeSpeed, float initialT)
    {
        t = initialT;
        this.endOfLineColor = endOfLineColor;
        this.frontOfLineColor = frontOfLineColor;
        this.timeToStart = timeToStart;
        this.colorChangeSpeed = colorChangeSpeed;
        play = true;
    }

    public void SetColor(Color color)
    {
        Vector3 start = LineRenderer.GetPosition(0);
        Vector3 end = LineRenderer.GetPosition(LineRenderer.positionCount - 1);

        Vector3[] positions = new Vector3[2];
        positions[0] = start;
        positions[1] = end;

        LineRenderer.positionCount = positions.Length;
        LineRenderer.SetPositions(positions);

        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Fixed;
        GradientColorKey[] gradientColorKeys = new GradientColorKey[3];
        GradientAlphaKey[] gradientAlphaKeys = new GradientAlphaKey[2];

        gradientColorKeys[0].color = color;
        gradientColorKeys[0].time = 0;

        gradientColorKeys[1].color = color;
        gradientColorKeys[1].time = 1;

        gradientAlphaKeys[0].alpha = color.a;
        gradientAlphaKeys[0].time = 0;

        gradientAlphaKeys[1].alpha = color.a;
        gradientAlphaKeys[1].time = 1;

        gradient.SetKeys(gradientColorKeys, gradientAlphaKeys);

        LineRenderer.colorGradient = gradient;
    }

    public void SetColorGradient(float t)
    {
        Vector3 start = LineRenderer.GetPosition(0);
        Vector3 end = LineRenderer.GetPosition(LineRenderer.positionCount - 1);

        Vector3 position = Vector3.Lerp(start, end, Mathf.Clamp01(t));

        Vector3 direction = (end - start).normalized;

        Vector3[] positions = new Vector3[4];
        positions[0] = start;
        positions[1] = position - direction * gap / 2f ;
        positions[2] = position + direction * gap / 2f;
        positions[3] = end;

        LineRenderer.positionCount = positions.Length;
        LineRenderer.SetPositions(positions);

        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Fixed;
        GradientColorKey[] gradientColorKeys = new GradientColorKey[3];
        GradientAlphaKey[] gradientAlphaKeys = new GradientAlphaKey[2];

        gradientColorKeys[0].color = frontOfLineColor;
        gradientColorKeys[0].time = 0;

        gradientColorKeys[1].color = frontOfLineColor;
        gradientColorKeys[1].time = t;

        gradientColorKeys[2].color = endOfLineColor;
        gradientColorKeys[2].time = 1;

        gradientAlphaKeys[0].alpha = frontOfLineColor.a;
        gradientAlphaKeys[0].time = 0;

        gradientAlphaKeys[1].alpha = endOfLineColor.a;
        gradientAlphaKeys[1].time = 1;

        gradient.SetKeys(gradientColorKeys, gradientAlphaKeys);

        LineRenderer.colorGradient = gradient;
        
    }
}
