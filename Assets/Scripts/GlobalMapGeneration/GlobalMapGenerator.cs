using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Map;

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

            GlobalMapPoint point = new GlobalMapPoint(position);

            combinedPoints.Add(point);
        }

        for (int i = Mathf.CeilToInt(-numSeperatedPaths / 2f); i < Mathf.CeilToInt(numSeperatedPaths / 2f); i++)
        {
            Vector2Int position = new Vector2Int(i, startLineDist - 1);

            GlobalMapPoint point = new GlobalMapPoint(position);

            combinedPoints.Add(point);
        }
    }


    private void OnDrawGizmos()
    {
        if (combinedPoints == null)
            return;

        foreach (GlobalMapPoint point in combinedPoints)
        {
            Gizmos.DrawCube((Vector2)point.point, Vector3.one * 0.5f);
        }
    }
}

[Serializable]
public class GlobalMapPoint
{
    public Vector2Int point;

    public LocalMapGenerationSettings localMapSettings;

    public GlobalMapPoint(Vector2Int point)
    {
        this.point = point;
    }

    public void LoadLocalMap()
    {
        SceneManager.LoadSceneAsync("LocalMap");
        LocalMapGenerator.Instance.biomeSettings = localMapSettings;
        LocalMapGenerator.Instance.CreateMap();
    }
}
