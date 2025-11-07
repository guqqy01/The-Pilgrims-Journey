using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;
[RequireComponent(typeof(LineRenderer))]
public class Beam : MonoBehaviour
{
    [SerializeField]
    private LineRenderer MainBeam;

    public Transform Target;

    [SerializeField]
    private LineRenderer[] lines;

    [SerializeField]
    private int lod;
    [SerializeField]
    private float amplitude;
    [SerializeField]
    private float frequency;

    [SerializeField]
    private float outerRadius;

    private Vector3 StartPosition => transform.position + (Target.position - transform.position).normalized * outerRadius;

    public void Update()
    {
        if(Target == null)
        {
            if(MainBeam.positionCount > 0)
                DeactivateBeam();

            return;
        }

        SetMainBeam();
        SetSubBeams();
    }

    private void DeactivateBeam()
    {
        MainBeam.positionCount = 0;

        foreach(var line in lines)
        {
            line.positionCount = 0;
        }
    }

    private void SetMainBeam()
    {
        if(MainBeam.positionCount != 2)
            MainBeam.positionCount = 2;

        MainBeam.SetPositions(new Vector3[] { StartPosition, Target.position });
    }

    private void SetSubBeams()
    {
        if(lod < 2)
            lod = 2;

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            LineRenderer lr = lines[lineIndex];

            if (lr.positionCount != lod)
                lr.positionCount = lod;

            Vector3[] positions = new Vector3[lod];

            for (int i = 0; i < lr.positionCount; i++)
            {
                Vector3 positionOnMainBeam = Vector3.Lerp(StartPosition, Target.position, i / (lod - 1f));
                Vector3 normalVector = Vector3.Cross(Target.position - transform.position, Vector3.forward).normalized;
                
                positions[i] = positionOnMainBeam + amplitude * (lineIndex + 1) * Mathf.Sin(frequency * (lineIndex + 1) * (lineIndex + 1) * Time.time + (i / (lod - 1f) + (lineIndex / (float)lines.Length)) * (lineIndex + 1) * 2f * Mathf.PI) * normalVector;
            }

            lr.SetPositions(positions);
        }
    }
}
