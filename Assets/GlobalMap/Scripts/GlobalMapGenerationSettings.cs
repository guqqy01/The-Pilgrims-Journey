using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GlobalMap
{
    [CreateAssetMenu()]
    public class GlobalMapGenerationSettings : ScriptableObject
    {
        public Vector2Int MapSize => new Vector2Int(InnerMapLength + StartRoadLength + EndRoadLength, MapWidth);
        public Vector2Int EndOfInnerMapPosition => new Vector2Int(StartRoadLength + InnerMapLength - 1, Mathf.FloorToInt(MapWidth / 2f));
        [SerializeField]
        private int InnerMapLength = 9;
        /// <summary>
        /// Must Be Odd
        /// </summary>
        [SerializeField]
        private int MapWidth = 7;

        public int StartRoadLength = 3;
        public int EndRoadLength = 3;

        public List<GlobalPathSettings> PathSettings = new List<GlobalPathSettings>();

        private float Time(float currentXPos) => (1f / (EndOfInnerMapPosition.x - StartRoadLength)) * (currentXPos - StartRoadLength);

        public bool Traverse(GlobalPathSettings path, int currentXPos) => path.Traverse(Time(currentXPos));
        public bool Converge(GlobalPathSettings path, int currentXPos) => path.Traverse(Time(currentXPos));
        public bool TendTowardsDirection(GlobalPathSettings path, int currentXPos) => path.Traverse(Time(currentXPos));

        public Direction GetNextDirection(GlobalPathSettings pathSettings, Direction currentDirection, Vector2Int currentPosition)
        {
            bool converge = pathSettings.Converge(Time(currentPosition.x));
            float currentValue = (float)currentPosition.y / (float)MapWidth;
            bool tendTowardsDirection = pathSettings.TendTowardsDirection(Time(currentPosition.x), out float tendValue);
            bool traverse = pathSettings.Traverse(Time(currentPosition.x));
            int abovePoint = AbovePoint(currentPosition.y);

            switch (currentDirection)
            {
                case Direction.North:
                    if (EdgeOfInnerMap(currentDirection, currentPosition))
                    {
                        if (pathSettings.Reverse)
                        {
                            currentDirection = Direction.West;
                        }
                        else
                        {
                            currentDirection = Direction.East;
                        }
                    }
                    else
                    {
                        if (EdgeOfInnerMap(Direction.East, currentPosition))
                            currentDirection = Direction.North;
                        else if (converge)
                        {
                            switch (abovePoint)
                            {
                                case -1:
                                    currentDirection = Direction.North;
                                    break;
                                default:
                                    if (pathSettings.Reverse)
                                    {
                                        currentDirection = Direction.West;
                                    }
                                    else
                                    {
                                        currentDirection = Direction.East;
                                    }
                                    break;
                            }
                        }
                        else if (traverse)
                        {
                            if (tendTowardsDirection)
                            {
                                if (currentValue > tendValue)
                                {
                                    if (pathSettings.Reverse)
                                    {
                                        currentDirection = Direction.West;
                                    }
                                    else
                                    {
                                        currentDirection = Direction.East;
                                    }
                                }
                                else
                                {
                                    currentDirection = Direction.North;
                                }
                            }
                            else
                            {
                                currentDirection = Direction.North;
                            }
                        }
                        else
                        {
                            if (pathSettings.Reverse)
                            {
                                currentDirection = Direction.West;
                            }
                            else
                            {
                                currentDirection = Direction.East;
                            }
                        }
                    }
                    break;
                case Direction.South:
                    if (EdgeOfInnerMap(currentDirection, currentPosition))
                    {
                        if (pathSettings.Reverse)
                        {
                            currentDirection = Direction.West;
                        }
                        else
                        {
                            currentDirection = Direction.East;
                        }
                    }
                    else
                    {
                        if (EdgeOfInnerMap(Direction.East, currentPosition))
                            currentDirection = Direction.South;
                        else if (converge)
                        {
                            switch (abovePoint)
                            {
                                case 1:
                                    currentDirection = Direction.South;
                                    break;
                                default:
                                    if (pathSettings.Reverse)
                                    {
                                        currentDirection = Direction.West;
                                    }
                                    else
                                    {
                                        currentDirection = Direction.East;
                                    }
                                    break;
                            }
                        }
                        else if (traverse)
                        {
                            if (tendTowardsDirection)
                            {
                                if (currentValue < tendValue)
                                {
                                    if (pathSettings.Reverse)
                                    {
                                        currentDirection = Direction.West;
                                    }
                                    else
                                    {
                                        currentDirection = Direction.East;
                                    }
                                }
                                else
                                {
                                    currentDirection = Direction.South;
                                }
                            }
                            else
                            {
                                currentDirection = Direction.South;
                            }
                        }
                        else
                        {
                            if (pathSettings.Reverse)
                            {
                                currentDirection = Direction.West;
                            }
                            else
                            {
                                currentDirection = Direction.East;
                            }
                        }
                    }
                    break;
                case Direction.East:
                    if (EdgeOfInnerMap(currentDirection, currentPosition))
                    {
                        switch (abovePoint)
                        {
                            case -1:
                                currentDirection = Direction.North;
                                break;
                            case 0:
                                currentDirection = Direction.East;
                                break;
                            case 1:
                                currentDirection = Direction.South;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (converge)
                        {
                            switch (abovePoint)
                            {
                                case -1:
                                    currentDirection = Direction.North;
                                    break;
                                case 0:
                                    currentDirection = Direction.East;
                                    break;
                                case 1:
                                    currentDirection = Direction.South;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (traverse)
                        {
                            if (tendTowardsDirection)
                            {
                                if (EdgeOfInnerMap(Direction.South, currentPosition))
                                {
                                    if (currentValue > tendValue)
                                    {
                                        currentDirection = Direction.East;
                                    }
                                    else
                                    {
                                        currentDirection = Direction.North;
                                    }
                                }
                                else if (EdgeOfInnerMap(Direction.North, currentPosition))
                                {
                                    if (currentValue > tendValue)
                                    {
                                        currentDirection = Direction.South;
                                    }
                                    else
                                    {
                                        currentDirection = Direction.East;
                                    }
                                }
                                else
                                {
                                    if(currentValue > tendValue)
                                    {
                                        currentDirection = Direction.South;
                                    }
                                    else
                                    {
                                        currentDirection = Direction.North;
                                    }
                                }
                            }
                            else
                            {
                                if (EdgeOfInnerMap(Direction.South, currentPosition))
                                {
                                    currentDirection = Direction.North;
                                }
                                else if (EdgeOfInnerMap(Direction.North, currentPosition))
                                {
                                    currentDirection = Direction.South;
                                }
                                else
                                {
                                    currentDirection = currentDirection.TangentDirections().GetRandom();
                                }
                            }
                        }
                        else
                        {
                            currentDirection = Direction.East;
                        }
                    }
                    break;
                case Direction.West:
                    if (EdgeOfInnerMap(currentDirection, currentPosition))
                    {
                        if (converge)
                        {
                            switch (abovePoint)
                            {
                                case -1:
                                    currentDirection = Direction.North;
                                    break;
                                case 0:
                                    currentDirection = currentDirection.TangentDirections().GetRandom();
                                    break;
                                case 1:
                                    currentDirection = Direction.South;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            currentDirection = currentDirection.TangentDirections().GetRandom();
                        }
                    }
                    else
                    {
                        if (pathSettings.Reverse)
                        {
                            currentDirection = Direction.West;
                        }
                        else
                        {
                            if (converge)
                            {
                                switch (abovePoint)
                                {
                                    case -1:
                                        currentDirection = Direction.North;
                                        break;
                                    case 0:
                                        currentDirection = currentDirection.TangentDirections().GetRandom();
                                        break;
                                    case 1:
                                        currentDirection = Direction.South;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                currentDirection = currentDirection.TangentDirections().GetRandom();
                            }
                        }
                    }
                    break;
            }

            return currentDirection;
        }

        bool EdgeOfInnerMap(Direction direction, Vector2Int position)
        {
            switch (direction)
            {
                case Direction.North:
                    return (position.y >= MapSize.y - 1);
                case Direction.South:
                    return (position.y <= 0);
                case Direction.East:
                    return (position.x >= EndOfInnerMapPosition.x);
                case Direction.West:
                    return (position.x <= StartRoadLength - 1);
            }
            return false;
        }

        
        /// <returns>1 if above point, 0 if at point, -1 if below Point</returns>
        int AbovePoint(int positionY)
        {
            if (positionY > Mathf.FloorToInt(MapSize.y / 2f))
            {
                return 1;
            }
            else if (positionY == Mathf.FloorToInt(MapSize.y / 2f))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}
