using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobalMapGenerator : MonoBehaviour
{
    public static GlobalMapGenerator instance;

    [SerializeField]
    private int startLineDist;
    [SerializeField]
    private int endLineDist;

    [SerializeField]
    private int numSeperatedPaths;
    [SerializeField]
    private int seperatedPathDistance;
    [SerializeField]
    private int seperatedPathSpacing = 5;

    [SerializeField]
    private List<GlobalMapPoint> combinedPoints = new List<GlobalMapPoint>();

    private List<List<Vector2Int>> paths = new List<List<Vector2Int>>();

    public void GenerateMap()
    {
        combinedPoints = new List<GlobalMapPoint>();
        for (int i = 0; i < startLineDist; i++)
        {
            Vector2Int position = new Vector2Int(0, i);
        }

        for (int i = Mathf.CeilToInt(-numSeperatedPaths / 2f); i < Mathf.CeilToInt(numSeperatedPaths / 2f); i++)
        {
            Vector2Int position = new Vector2Int(i, startLineDist - 1);
        }
    }

}
