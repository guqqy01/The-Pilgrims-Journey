using System;
using UnityEngine;
namespace LocalMap
{
    [Serializable]
    public class MapObject
    {
        public string name;

        public GameObject overlay;

        public Vector2Int size;

        public AnimationCurve amountToSpawnCurve;

        public TileType validPlacementTiles;
    }
}
