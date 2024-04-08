using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityEngine.ParticleSystem;

namespace Map
{
    [CreateAssetMenu()]
    public class LocalMapGenerationSettings : ScriptableObject
    {
        [SerializeField]
        private WeightedChanceList<GameObject> mapTiles;

        [SerializeField]
        private WeightedChanceList<GameObject> pathTiles;

        public List<OverlayMapObject> mapObjects;

        public MinMaxCurve mapSize;

        public MinMaxCurve targetDirectionVSLevelCurve;
        public MinMaxCurve traverseDirectionVSLevelCurve;


        public GameObject GetMapTile()
        {
            return mapTiles.GetRandom();
        }

        public GameObject GetPathTile()
        {
            return pathTiles.GetRandom();
        }

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


