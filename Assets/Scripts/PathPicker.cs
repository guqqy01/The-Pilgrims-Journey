using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPicker : MonoBehaviour
{
    public LocalMapGenerator MapGenerator
    {
        get
        {
            if (mapGenerator == null)
                mapGenerator = LocalMapGenerator.Instance;

            return mapGenerator;
        }
    }
    private LocalMapGenerator mapGenerator;

    public InputManager InputManagerInstance
    {
        get
        {
            if (inputManager == null)
                inputManager = InputManager.Instance;

            return inputManager;
        }
    }

    private InputManager inputManager;

    public static PathPicker instance { private set; get; }

    [SerializeField]
    private LineRenderer lr;

    [SerializeField]
    private GameObject pathPickerPointPrefab;

    [SerializeField]
    private LayerMask pathPickerLayerMask;

    private Dictionary<Direction, Vector2Int> pathEndPoints;

    private Dictionary<Transform, Direction> pathPickerPoints = new Dictionary<Transform, Direction>();

    public Direction? pickedDirection = null;

    [SerializeField]
    private Camera cam;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (InputManagerInstance.LeftClick)
        {
            Vector3 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D collider = Physics2D.OverlapPoint(worldPoint, pathPickerLayerMask);

            if (collider != null)
            {
                if (pathPickerPoints.ContainsKey(collider.transform))
                {
                    Direction direction = pathPickerPoints[collider.transform];

                    List<Vector2Int> pathPoints = mapGenerator.pathway.PathNodes[direction];

                    SetPositions(pathPoints);

                    pickedDirection = direction;
                }
            }
        }
    }

    public void Setup()
    {
        pathEndPoints = MapGenerator.pathway.PathEndPoints;

        foreach (KeyValuePair<Direction, Vector2Int> endPoint in pathEndPoints)
        {
            GameObject pickerPoint = Instantiate(pathPickerPointPrefab, (Vector2)endPoint.Value, Quaternion.identity, transform);
            pathPickerPoints.Add(pickerPoint.transform, endPoint.Key);
        }
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
