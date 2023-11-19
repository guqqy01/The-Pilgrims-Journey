using UnityEngine;
using System;
using System.Collections.Generic;
namespace Map
{
    [CreateAssetMenu()]
    public class BiomeSettings : ScriptableObject
    {
        public List<MapTile> mapTiles;
        [SerializeField, HideInInspector]
        private int maxMapTileChance = 0;

        public List<PathTile> pathTiles;
        [SerializeField, HideInInspector]
        private int maxPathTileChance = 0;

        public List<OverlayMapObject> mapObjects;

        public Vector2Int mapSize;

        public Vector2Int targetDirectionMinMax = new Vector2Int(2, 5);
        public Vector2Int traverseDirectionMinMax = new Vector2Int(2, 10);

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

        public bool TryGetOverlayObject(Dictionary<string, int> inputMapObjects, out OverlayMapObject overlayMapObject)
        {
            overlayMapObject = new OverlayMapObject();

            for (int i = 0; i < mapObjects.Count; i++)
            {
                if (inputMapObjects.ContainsKey(mapObjects[i].name) && inputMapObjects[mapObjects[i].name] >= mapObjects[i].maxAmount)
                    continue;

                float rnd = UnityEngine.Random.value;
                if (mapObjects[i].chance >= rnd)
                {
                    overlayMapObject = mapObjects[i];
                    return true;
                }
            }

            return false;
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

        [Range(0, 1)]
        public float chance;

        public int maxAmount;
    }
}


