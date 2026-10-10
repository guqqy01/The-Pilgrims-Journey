using UnityEngine;
using System.Collections.Generic;
using System;
using LocalMap;

namespace GlobalMap
{
    public class GlobalMapPoint
    {
        public Vector2Int Position;
        public int Seed;

        public LocalMapGenerationSettings GenerationSettings;

        public GlobalMapPointType Type = GlobalMapPointType.Normal;

        public Dictionary<Direction, GlobalMapPoint> connectedFromPoints = new Dictionary<Direction, GlobalMapPoint>();
        public Dictionary<Direction, GlobalMapPoint> connectedToPoints = new Dictionary<Direction, GlobalMapPoint>();

        public static bool HasPointsConnectedBothWays(GlobalMapPoint point1, GlobalMapPoint point2)
        {
            return point1.connectedToPoints.ContainsValue(point2) && point2.connectedToPoints.ContainsValue(point1);
        }

        public bool HasPointsConnectedBothWays(GlobalMapPoint otherPoint)
        {
            return HasPointsConnectedBothWays(this, otherPoint);
        }

        public GlobalMapPoint(Vector2Int Position)
        {
            this.Position = Position;
        }

        public List<Direction> GetOtherToDirections(Direction startWall)
        {
            List<Direction> returnList = new List<Direction>();

            foreach(var kvp in connectedToPoints)
            {
                if (kvp.Key == startWall)
                    continue;

                returnList.Add(kvp.Key);
            }

            return returnList;
        }

        public List<Direction> GetOtherFromDirections(Direction startWall)
        {
            List<Direction> returnList = new List<Direction>();

            foreach (var kvp in connectedFromPoints)
            {
                if (kvp.Key == startWall)
                    continue;

                returnList.Add(kvp.Key);
            }

            return returnList;
        }


        public void LoadScene(Action onSceneLoaded = null)
        {
            SceneLoader.Instance.LoadSceneFromType(Type.GetSceneLoadType(), onSceneLoaded);
        }
    }

    [Serializable]
    public enum GlobalMapPointType
    {
        Normal,
        Random,
        Shop,
        Relic,
    }

    public static class GlobalMapPointTypeExtensions
    {
        public static SceneLoad GetSceneLoadType(this GlobalMapPointType type)
        {
            switch (type)
            {
                case GlobalMapPointType.Normal:
                    return SceneLoad.LocalMap;
                case GlobalMapPointType.Random:
                    return SceneLoad.RandomEvent;
                case GlobalMapPointType.Shop:
                    return SceneLoad.Shop;
                case GlobalMapPointType.Relic:
                    return SceneLoad.LocalMap;
                default:
                    throw new NotImplementedException($"No scene load type defined for {type}");
            }
        }
    }
}

