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

        /*private float Time(float currentXPos) => (1f / (EndOfInnerMapPosition.x - StartRoadLength)) * (currentXPos - StartRoadLength);*/

        private float Time(float currentXPos)
        {
            float denom = Mathf.Max(1f, (EndOfInnerMapPosition.x - StartRoadLength));
            return Mathf.Clamp01((currentXPos - StartRoadLength) / denom);
        }

        private void OnValidate()
        {
            if(MapWidth % 2 == 0)
            {
                MapWidth += 1;
            }

            MapWidth = Mathf.Max(1, MapWidth);
        }

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
                    currentDirection = HandleVerticalMovement(currentDirection, pathSettings, currentPosition, true);
                    break;
                case Direction.South:
                    currentDirection = HandleVerticalMovement(currentDirection, pathSettings, currentPosition, false);
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
            }

            return currentDirection;
        }

        Direction HandleVerticalMovement(Direction currentDirection, GlobalPathSettings pathSettings, Vector2Int position, bool isNorth)
        {
            bool converge = pathSettings.Converge(Time(position.x));
            bool traverse = pathSettings.Traverse(Time(position.x));
            bool tend = pathSettings.TendTowardsDirection(Time(position.x), out float tendValue);
            float currentValue = (float)position.y / MapWidth;
            int abovePoint = AbovePoint(position.y);

            if (EdgeOfInnerMap(isNorth ? Direction.North : Direction.South, position))
                return Direction.East;

            if (EdgeOfInnerMap(Direction.East, position))
                return currentDirection;

            if (converge)
            {
                if (abovePoint == (isNorth ? -1 : 1))
                    return currentDirection;
                return Direction.East;
            }

            if (traverse)
            {
                if (tend)
                {
                    bool condition = isNorth ? currentValue > tendValue : currentValue < tendValue;
                    return condition ? Direction.East : currentDirection;
                }
                return currentDirection;
            }

            return Direction.East;
        }


        bool EdgeOfInnerMap(Direction direction, Vector2Int position) => direction switch
        {
            Direction.North => position.y >= MapSize.y - 1,
            Direction.South => position.y <= 0,
            Direction.East => position.x >= EndOfInnerMapPosition.x,
            Direction.West => position.x <= StartRoadLength - 1,
            _ => false
        };


        /// <returns>1 if above point, 0 if at point, -1 if below Point</returns>
        int AbovePoint(int y)
        {
            int mid = Mathf.FloorToInt(MapSize.y / 2f);
            return y.CompareTo(mid);
        }
    }
}
