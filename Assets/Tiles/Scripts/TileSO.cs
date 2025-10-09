using System.Collections.Generic;
using UnityEngine;

namespace Tiles
{
    [CreateAssetMenu()]
    public class TileSO : ScriptableObject
    {
        public Sprite TileSprite;

        [SerializeField]
        private Vector2ListWrapper shape;
        public List<Vector2Int> Shape => shape.v2IntList;

        public Tile tilePrefab;

        public static float amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier = 10;

        [SerializeField]
        private Rarity rarity;
        public Rarity Rarity => rarity;
    }
}

