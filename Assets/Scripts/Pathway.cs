using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class Pathway
    {
        public Dictionary<Direction, List<Vector2Int>> Nodes;

        public Dictionary<Direction, List<Vector2Int>> Paths;

        public int Count => Nodes.Count;

        public bool ContainsNode(Vector2Int vector2Int)
        {
            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in Nodes)
            {
                if (directionAndPath.Value.Contains(vector2Int))
                    return true;
            }
            return false;
        }

        public bool ContainsStep(Vector2Int vector2Int)
        {
            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in Paths)
            {
                if (directionAndPath.Value.Contains(vector2Int))
                    return true;
            }
            return false;
        }

        public Pathway(List<Direction> targetDirections)
        {
            Nodes = new Dictionary<Direction, List<Vector2Int>>();

            for (int i = 0; i < targetDirections.Count; i++)
            {
                Nodes.Add(targetDirections[i], new List<Vector2Int>());
            }
        }

        public List<Vector2Int> this[Direction direction]
        {
            get
            {
                return Nodes[direction];
            }
        }

        public Vector2Int this[Direction direction, int point]
        {
            get
            {
                return Nodes[direction][point];
            }
            set
            {
                Nodes[direction][point] = value;
            }
        }

        public List<Vector2Int> GetPathFromNodes(List<Vector2Int> nodes)
        {
            List<Vector2Int> path = new List<Vector2Int>();

            for (int i = 0; i < nodes.Count; i++)
            {
                if (!path.Contains(nodes[i]))
                    path.Add(nodes[i]);


                if (i != nodes.Count - 1)
                {
                    int xPos1 = nodes[i].x;
                    int yPos1 = nodes[i].y;

                    int xPos2 = nodes[i + 1].x;
                    int yPos2 = nodes[i + 1].y;

                    for (int k = 1; k < Mathf.Abs(xPos1 - xPos2); k++)
                    {
                        path.Add(new Vector2Int(xPos1 - (k * (int)Mathf.Sign(xPos1 - xPos2)), yPos1));
                    }

                    for (int k = 1; k < Mathf.Abs(yPos1 - yPos2); k++)
                    {
                        path.Add(new Vector2Int(xPos1, yPos1 - (k * (int)Mathf.Sign(yPos1 - yPos2))));
                    }
                }
            }

            return path;
        }

        public void CreatePaths(List<Direction> targetDirections)
        {
            if (targetDirections.Count == 0)
                return;

            Paths = new Dictionary<Direction, List<Vector2Int>>();
            List<Vector2Int> mainPath = GetPathFromNodes(Nodes[targetDirections[0]]);
            Paths.Add(targetDirections[0], mainPath);

            if (targetDirections.Count == 1)
                return;

            for (int i = 1; i < targetDirections.Count; i++)
            {
                Vector2Int connectionNode = Nodes[targetDirections[i]][0];

                List<Vector2Int> path = new List<Vector2Int>();

                foreach(Vector2Int step in mainPath)
                {
                    if (step == connectionNode)
                        break;

                    path.Add(step);
                }

                path.AddRange(GetPathFromNodes(Nodes[targetDirections[i]]));

                Paths.Add(targetDirections[i], path);
            }
        }
    }
}


