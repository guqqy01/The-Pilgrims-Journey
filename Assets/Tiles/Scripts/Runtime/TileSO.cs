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
        public List<Vector2Int> Shape => shape;

        public TileUI tilePrefab;

        public static float amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier = 1;

        [SerializeField]
        private Rarity rarity;
        public Rarity Rarity => rarity;
    }
}

