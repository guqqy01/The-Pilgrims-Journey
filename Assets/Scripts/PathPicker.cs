using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPicker : MonoBehaviour
{
    public static PathPicker instance { private set; get; }

    [SerializeField]
    private LineRenderer lr;


    private void Awake()
    {
        instance = this;
    }

    public void SetPositions(List<Vector2Int> points)
    {
        Vector3[] positions = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            positions[i] = (Vector2)points[i];
        }
        lr.positionCount = positions.Length;
        lr.SetPositions(positions);
    }
}
