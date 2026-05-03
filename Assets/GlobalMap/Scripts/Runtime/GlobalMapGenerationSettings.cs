using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using LocalMap;

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

        [Header("Biome Settings")]
        public LocalMapGenerationSettings startingBiome;
        public List<BiomeEntry> biomePool = new List<BiomeEntry>();

        [Tooltip("Minimum number of consecutive map points that must share the same biome before it can change.")]
        public int minSameSettingsInARow = 3;

        [Tooltip("Chance per point that the biome changes, once the minimum run length is met.")]
        [Range(0f, 1f)] public float biomeChangeChance = 0.15f;

        [Tooltip("How much to penalise picking the same biome as the current point. 0 = no penalty, 1 = impossible.")]
        [Range(0f, 1f)] public float sameAsCurrentPenalty = 0.7f;

        [Tooltip("How much to penalise picking the same biome as a sibling exit on the same point. 0 = no penalty, 1 = impossible.")]
        [Range(0f, 1f)] public float sameAsSiblingPenalty = 0.5f;

        private float Time(float currentXPos)
        {
            float denom = Mathf.Max(1f, EndOfInnerMapPosition.x - StartRoadLength);
            return Mathf.Clamp01((currentXPos - StartRoadLength) / denom);
        }

        private void OnValidate()
        {
            if(MapWidth % 2 == 0)
                MapWidth += 1;

            MapWidth = Mathf.Max(1, MapWidth);
        }

        public LocalMapGenerationSettings PickBiome(
            float xProgress,
            LocalMapGenerationSettings currentBiome,
            List<LocalMapGenerationSettings> siblingBiomes)
        {
            List<(LocalMapGenerationSettings settings, float weight)> candidates
                = new List<(LocalMapGenerationSettings, float)>();

            foreach (BiomeEntry entry in biomePool)
            {
                if (entry.settings == null) continue;

                float weight = entry.progressWeightCurve.Evaluate(xProgress);

                if (entry.settings == currentBiome)
                    weight *= (1f - sameAsCurrentPenalty);

                foreach (LocalMapGenerationSettings sibling in siblingBiomes)
                {
                    if (entry.settings == sibling)
                    {
                        weight *= (1f - sameAsSiblingPenalty);
                        break;
                    }
                }

                if (weight > 0f)
                    candidates.Add((entry.settings, weight));
            }

            if (candidates.Count == 0)
                return currentBiome;

            float total = 0f;
            foreach (var c in candidates) total += c.weight;

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            foreach (var (settings, weight) in candidates)
            {
                cumulative += weight;
                if (roll <= cumulative)
                    return settings;
            }

            return candidates[candidates.Count - 1].settings;
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

    [System.Serializable]
    public class BiomeEntry
    {
        public LocalMapGenerationSettings settings;
        [Tooltip("X axis = 0-1 progress along the map. Y axis = relative weight of this biome at that point.")]
        public AnimationCurve progressWeightCurve = AnimationCurve.Constant(0f, 1f, 1f);
    }
}
