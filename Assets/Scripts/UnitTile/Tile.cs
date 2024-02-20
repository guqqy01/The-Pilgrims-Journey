using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Units;

[RequireComponent(typeof(TileStats))]
public class Tile : MonoBehaviour
{
    public bool locked;

    public UnitSO unit;

    public TileShape shape;

    public TileStats tileStats { private set; get; }

    public bool Selected { private set; get; } = false;

    [SerializeField]
    private RectTransform unitUITransform;

    public RectTransform rectTransform { private set; get; }
    [SerializeField]
    private Image image;

    private static float multiplier = 210;
    public Vector2 Offset => multiplier * GetOffset();

    public float TimeForNextSpawn = 0;
    public float SpawnRate = 0;

    public Vector2 GetOffset()
    {
        int xMin = int.MaxValue, xMax = int.MinValue, yMin = int.MaxValue, yMax = int.MinValue;

        foreach (Vector2Int v2int in Shape)
        {
            if (v2int.x > xMax)
                xMax = v2int.x;

            if (v2int.x < xMin)
                xMin = v2int.x;

            if (v2int.y > yMax)
                yMax = v2int.y;

            if (v2int.y < yMin)
                yMin = v2int.y;
        }
        Vector2 v2 = new Vector2((xMax + xMin) / 2f, (yMax + yMin) / 2f);
        return v2;
    }

    public List<Vector2Int> Shape
    {
        get
        {
            float rotation = rectTransform.rotation.eulerAngles.z;

            List<Vector2Int> returnShape = new List<Vector2Int>();

            for (int i = 0; i < shape.shape.v2IntList.Count; i++)
            {
                int x = Mathf.RoundToInt(shape.shape.v2IntList[i].x * Mathf.Cos(rotation * Mathf.Deg2Rad) - shape.shape.v2IntList[i].y * Mathf.Sin(rotation * Mathf.Deg2Rad));
                int y = Mathf.RoundToInt(shape.shape.v2IntList[i].x * Mathf.Sin(rotation * Mathf.Deg2Rad) + shape.shape.v2IntList[i].y * Mathf.Cos(rotation * Mathf.Deg2Rad));
                returnShape.Add(new Vector2Int(x, y));
            }


            return returnShape;
        }
    }

    public Color Color
    {
        set
        {
            image.color = value;
        }
    }

    public void Rotate(int angle)
    {
        rectTransform.Rotate(new Vector3(0, 0, angle));
        unitUITransform.rotation = Quaternion.Euler(Vector3.zero);
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        tileStats = GetComponent<TileStats>();
    }

    public void SelectTile(bool select)
    {
        Selected = select;
    }
}
