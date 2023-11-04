using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Map
{
    public class LocalMapGenerator : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int mapSize = new Vector2Int(20, 20);

        [SerializeField]
        private float mapScale = 1f;

        [SerializeField]
        private Color gizmosColor;

        public List<Vector2Int> path;

        public Vector2Int start = new Vector2Int(10, 0);

        private void Start()
        {
            CreatePath();
        }

        public void CreatePath()
        {
            path = new List<Vector2Int>() {start};

            int yPos = 0;

            Direction currentDirection = Direction.Up;

            while (yPos < mapSize.y)
            {
                int xPos = path[path.Count - 1].x;
                switch (currentDirection)
                {
                    case Direction.Up:
                        yPos = Mathf.Min(UnityEngine.Random.Range(yPos + 2, yPos + 8), mapSize.y);
                        float dirRan = UnityEngine.Random.Range(1f / mapSize.x, 1f - 1f / mapSize.x);
                        currentDirection = (dirRan > (float)xPos / mapSize.x) ? Direction.Right: Direction.Left;
                        break;
                    case Direction.Left:
                        xPos = Mathf.Max(UnityEngine.Random.Range(xPos - 8, xPos - 2), 1);
                        currentDirection = Direction.Up;
                        break;
                    case Direction.Right:
                        xPos = Mathf.Min(UnityEngine.Random.Range(xPos + 2, xPos + 8), mapSize.x - 1);
                        currentDirection = Direction.Up;
                        break;
                }

                path.Add(new Vector2Int(xPos, yPos));
            }
        }

        public void GenerateMap()
        {

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmosColor;
            Gizmos.DrawWireCube(Vector3.zero, (Vector2)mapSize * mapScale);

            for (int i = 0; i < path.Count; i++)
            {
                Vector2 pos = ((Vector2)path[i] - mapSize / 2) * mapScale;
                Gizmos.DrawCube(pos, Vector3.one * mapScale * 0.5f);

                if (i != path.Count - 1)
                    Gizmos.DrawLine(pos, ((Vector2)path[i + 1] - mapSize / 2) * mapScale);
            }
        }
    }

    [Flags]
    public enum Direction
    {
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
    }
}


