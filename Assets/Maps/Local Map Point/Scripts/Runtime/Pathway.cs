using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LocalMap
{
    public class Pathway
    {
        /// <summary>
        /// All the steps in the path sections, a path section is where the branching path deviates from the initial path. There wont be any repeating points between directions.
        /// Used for map creation
        /// </summary>
        public Dictionary<Direction, List<Vector2Int>> SectionNodesPaths;

        /// <summary>
        /// All points on a path, there will be repeating points between directions.
        /// </summary>
        public Dictionary<Direction, List<Vector2Int>> StepPaths;

        /// <summary>
        /// All the points on a path that have a change in direction, including start and finish nodes
        /// </summary>
        public Dictionary<Direction, List<Vector2Int>> PathNodes;

        /// <summary>
        /// The end point of all the paths that are avaliable to go
        /// </summary>
        public Dictionary<Direction, Vector2Int> ToEndPoints;
        public Dictionary<Direction, Vector2Int> UnavaliablePathEndPoints;

        public List<Direction> AllDirections {  get; private set; }

        public List<Direction> ToDirections { private set; get; }
        public List<Direction> OtherFromDirections { private set; get; }
      
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
            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in StepPaths)
            {
                if (directionAndPath.Value.Contains(vector2Int))
                    return true;
            }
            return false;
        }

        public bool ContainsStep(int x, int y)
        {
            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in StepPaths)
            {
                if (directionAndPath.Value.Contains(new Vector2Int(x, y)))
                    return true;
            }
            return false;
        }

        public Pathway(List<Direction> targetDirections, List<Direction> otherFromDirections)
        {
            ToDirections = targetDirections;
            OtherFromDirections = otherFromDirections;

            AllDirections = new List<Direction>();
            AllDirections.AddRange(targetDirections);
            AllDirections.AddRange(otherFromDirections);

            SectionNodesPaths = new Dictionary<Direction, List<Vector2Int>>();

            for (int i = 0; i < targetDirections.Count; i++)
            {
                SectionNodesPaths.Add(targetDirections[i], new List<Vector2Int>());
            }

            for (int i = 0; i < otherFromDirections.Count; i++)
            {
                SectionNodesPaths.Add(otherFromDirections[i], new List<Vector2Int>());
            }
        }

        public List<Vector2Int> this[Direction direction]
        {
            get
            {
                return SectionNodesPaths[direction];
            }
        }

        public Vector2Int this[Direction direction, int index]
        {
            get
            {
                return SectionNodesPaths[direction][index];
            }
            set
            {
                SectionNodesPaths[direction][index] = value;
            }
        }


        private List<Vector2Int> GetPathFromNodes(List<Vector2Int> nodes)
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

        private void CreateNodePaths()
        {
            if (AllDirections.Count == 0)
                return;

            PathNodes = new Dictionary<Direction, List<Vector2Int>>
            {
                { AllDirections[0], SectionNodesPaths[AllDirections[0]] }
            };

            if (AllDirections.Count == 1)
                return;

            for (int i = 1; i < AllDirections.Count; i++)
            {
                Vector2Int connectionNode = SectionNodesPaths[AllDirections[i]][0];

                List<Vector2Int> path = new List<Vector2Int>();

                int? connectionIndex = null;

                for (int j = 0; j < i; j++)
                {
                    if (SectionNodesPaths[AllDirections[j]].Contains(connectionNode))
                    {
                        connectionIndex = j;
                        break;
                    }
                }

                for (int j = 0; j <= connectionIndex; j++)
                {
                    Vector2Int stepConnectionNode = j < connectionIndex ? SectionNodesPaths[AllDirections[j + 1]][0] : connectionNode;

                    foreach (Vector2Int node in SectionNodesPaths[AllDirections[j]])
                    {
                        if (node == stepConnectionNode)
                            break;

                        path.Add(node);
                    }
                }

                path.AddRange(SectionNodesPaths[AllDirections[i]]);

                PathNodes.Add(AllDirections[i], path);
            }
        }

        public void CreatePaths()
        {
            CreateNodePaths();
            StepPaths = new Dictionary<Direction, List<Vector2Int>>();
            for (int i = 0; i < AllDirections.Count; i++)
            {
                List<Vector2Int> stepPath = GetPathFromNodes(PathNodes[AllDirections[i]]);

                StepPaths.Add(AllDirections[i], stepPath);
            }

            ToEndPoints = new Dictionary<Direction, Vector2Int>();

            foreach(Direction toDirection in ToDirections)
            {
                ToEndPoints.Add(toDirection, SectionNodesPaths[toDirection].Last());
            }
        }
    }
}


