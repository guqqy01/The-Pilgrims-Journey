using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityEngine.ParticleSystem;

namespace Map
{
    [CreateAssetMenu()]
    public class LocalMapGenerationSettings : ScriptableObject
    {
        public List<MapTile> mapTiles;
        [SerializeField, HideInInspector]
        private int maxMapTileChance = 0;

        public List<PathTile> pathTiles;
        [SerializeField, HideInInspector]
        private int maxPathTileChance = 0;

        public List<OverlayMapObject> mapObjects;

        public Vector2Int mapSize;

        public MinMaxCurve targetDirectionVSLevelCurve;
        public MinMaxCurve traverseDirectionVSLevelCurve;

        public void Setup()
        {
            maxMapTileChance = 0;
            maxPathTileChance = 0;

            for (int i = 0; i < mapTiles.Count; i++)
            {
                maxMapTileChance += mapTiles[i].sharedChance;
            }

            for (int i = 0; i < pathTiles.Count; i++)
            {
                maxPathTileChance += pathTiles[i].sharedChance;
            }
        }

        public GameObject GetMapTile()
        {
            int rnd = UnityEngine.Random.Range(1, maxMapTileChance);

            for (int i = 0; i < mapTiles.Count; i++)
            {
                rnd -= mapTiles[i].sharedChance;

                if (rnd <= 0)
                    return mapTiles[i].tile;
            }

            return mapTiles[0].tile;
        }

        public GameObject GetPathTile()
        {
            int rnd = UnityEngine.Random.Range(1, maxPathTileChance);

            for (int i = 0; i < pathTiles.Count; i++)
            {
                rnd -= pathTiles[i].sharedChance;

                if (rnd <= 0)
                    return pathTiles[i].tile;
            }

            return pathTiles[0].tile;
        }

    }

    [Serializable]
    public struct MapTile
    {
        public GameObject tile;

        public int sharedChance;
    }

    [Serializable]
    public struct PathTile
    {
        public GameObject tile;

        public int sharedChance;
    }

    [Serializable]
    public struct OverlayMapObject
    {
        public string name;

        public GameObject overlay;

        public Vector2Int size;

        public AnimationCurve amountToSpawnCurve;
    }
}


