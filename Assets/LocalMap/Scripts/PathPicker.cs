using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Tiles;
using GlobalMap;
using static LocalMap.PathPicker;

namespace LocalMap
{
    public class PathPicker : Singleton<PathPicker>
    {
        private LocalMapGenerator mapGenerator;
        public LocalMapGenerator MapGenerator
        {
            get
            {
                if (mapGenerator == null)
                    mapGenerator = LocalMapGenerator.Instance;

                return mapGenerator;
            }
        }


        private InputManager inputManager;
        public InputManager InputManagerInstance
        {
            get
            {
                if (inputManager == null)
                    inputManager = InputManager.Instance;

                return inputManager;
            }
        }

        [SerializeField]
        private LineRenderer lr;

        [SerializeField]
        private GameObject pathPickerPointPrefab;

        [SerializeField]
        private LayerMask pathPickerLayerMask;

        private Dictionary<Direction, Vector2Int> pathEndPoints;

        private Dictionary<Transform, Direction> pathPickerPoints = new Dictionary<Transform, Direction>();

        public Direction? PickedDirection { get; private set; } = null;

        private Camera cam;
        private Camera Cam
        {
            get
            {
                if(cam == null)
                    cam = Camera.main;

                return cam;
            }
        }

        public delegate void OnClickSign(Direction direction);
        public OnClickSign onClickSign;

        protected override void CleanUp()
        {
            onClickSign = null;
        }

        private void Update()
        {
            if (InputManagerInstance == null)
                return;

            if (InputManagerInstance.LeftClick)
            {
                Vector3 worldPoint = Cam.ScreenToWorldPoint(Input.mousePosition);
                Collider2D collider = Physics2D.OverlapPoint(worldPoint, pathPickerLayerMask);

                if (collider != null)
                {
                    if (pathPickerPoints.ContainsKey(collider.transform))
                    {
                        PickDirection(pathPickerPoints[collider.transform]);
                    }
                    else if (LocalMapGenerator.Instance.RoadSigns.ContainsKey(collider.gameObject))
                    {
                        onClickSign?.Invoke(LocalMapGenerator.Instance.RoadSigns[collider.gameObject]);
                    }
                }
            }
        }

        public void PickDirection(Direction direction)
        {
            if (LocalMapLevelCompletedManager.Instance.CurrentlyPlayingLocalLevel)
                return;

            if (!pathEndPoints.ContainsKey(direction))
                return;

            if(!GlobalMapGenerator.Instance.TryPickDirection(direction))
                return;

            List<Vector2Int> pathPoints = mapGenerator.pathway.PathNodes[direction];

            SetPositions(pathPoints);

            PickedDirection = direction;
        }

        public void Setup()
        {
            pathEndPoints = MapGenerator.pathway.ToEndPoints;
            SetPositions(new List<Vector2Int>());
            foreach (KeyValuePair<Transform, Direction> kvp in pathPickerPoints)
            {
                if (kvp.Key != null)
                    Destroy(kvp.Key.gameObject);
            }

            pathPickerPoints.Clear();

            foreach (KeyValuePair<Direction, Vector2Int> endPoint in pathEndPoints)
            {
                Vector2 spawnPosition = endPoint.Value;

                if (endPoint.Key == Direction.East)
                    spawnPosition -= new Vector2(1, 0);

                if (endPoint.Key == Direction.North)
                    spawnPosition -= new Vector2(0, 1);

                if (endPoint.Key == Direction.West)
                    spawnPosition += new Vector2(1, 0);

                if (endPoint.Key == Direction.South)
                    spawnPosition += new Vector2(0, 1);

                GameObject pickerPoint = Instantiate(pathPickerPointPrefab, spawnPosition, Quaternion.identity, transform);
                pathPickerPoints.Add(pickerPoint.transform, endPoint.Key);
            }

            if (pathPickerPoints.Count == 1)
            {
                Direction direction = pathPickerPoints.Values.ToList()[0];

                PickDirection(direction);
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

}