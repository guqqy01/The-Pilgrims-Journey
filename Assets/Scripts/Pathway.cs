using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Map
{
    public class Pathway
    {
        public Dictionary<Direction, List<Vector2Int>> SectionNodesPaths;

        public Dictionary<Direction, List<Vector2Int>> PathNodes;

        public Dictionary<Direction, List<Vector2Int>> stepPaths;

        public int Count => SectionNodesPaths.Count;
        public int PathCount(Direction direction) => PathNodes[direction].Count;

        public bool ContainsNode(Vector2Int vector2Int)
        {
            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in SectionNodesPaths)
            {
                if (directionAndPath.Value.Contains(vector2Int))
                    return true;
            }
            return false;
        }

        public bool ContainsStep(Vector2Int vector2Int)
        {
            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in stepPaths)
            {
                if (directionAndPath.Value.Contains(vector2Int))
                    return true;
            }
            return false;
        }

        public bool ContainsStep(int x, int y)
        {
            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in stepPaths)
            {
                if (directionAndPath.Value.Contains(new Vector2Int(x, y)))
                    return true;
            }
            return false;
        }

        public Pathway(List<Direction> targetDirections)
        {
            SectionNodesPaths = new Dictionary<Direction, List<Vector2Int>>();

            for (int i = 0; i < targetDirections.Count; i++)
            {
                SectionNodesPaths.Add(targetDirections[i], new List<Vector2Int>());
            }
        }

        public List<Vector2Int> this[Direction direction]
        {
            get
            {
                return SectionNodesPaths[direction];
            }
        }

        public Vector2Int this[Direction direction, int point]
        {
            get
            {
                return SectionNodesPaths[direction][point];
            }
            set
            {
                SectionNodesPaths[direction][point] = value;
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

        public void CreateNodePath(List<Direction> targetDirections)
        {
            if (targetDirections.Count == 0)
                return;

            PathNodes = new Dictionary<Direction, List<Vector2Int>>
            {
                { targetDirections[0], SectionNodesPaths[targetDirections[0]] }
            };

            if (targetDirections.Count == 1)
                return;

            for (int i = 1; i < targetDirections.Count; i++)
            {
                Vector2Int connectionNode = SectionNodesPaths[targetDirections[i]][0];

                List<Vector2Int> path = new List<Vector2Int>();

                int? connectionIndex = null;

                for (int j = 0; j < i; j++)
                {
                    if (SectionNodesPaths[targetDirections[j]].Contains(connectionNode))
                    {
                        connectionIndex = j;
                        break;
                    }
                }

                for (int j = 0; j <= connectionIndex; j++)
                {
                    Vector2Int stepConnectionNode = j < connectionIndex ? SectionNodesPaths[targetDirections[j + 1]][0] : connectionNode;

                    foreach (Vector2Int node in SectionNodesPaths[targetDirections[j]])
                    {
                        if (node == stepConnectionNode)
                            break;

                        path.Add(node);
                    }
                }

                path.AddRange(SectionNodesPaths[targetDirections[i]]);

                PathNodes.Add(targetDirections[i], path);
            }
        }

        public void CreatePaths(List<Direction> targetDirections)
        {
            CreateNodePath(targetDirections);
            stepPaths = new Dictionary<Direction, List<Vector2Int>>();
            for (int i = 0; i < targetDirections.Count; i++)
            {
                List<Vector2Int> stepPath = GetPathFromNodes(PathNodes[targetDirections[i]]);
                stepPaths.Add(targetDirections[i], stepPath);
            }
        }
    }
}


