using System.Collections.Generic;
using UnityEngine;

namespace Tiles
{
    [CreateAssetMenu(menuName = "Tiles/TileSO")]
    public class TileSO : ScriptableObject
    {
        [SerializeField]
        private Vector2ListWrapper shape;
        public List<Vector2Int> Shape => shape;

        public static float amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier = 1;

        [SerializeField]
        private Rarity rarity;
        public Rarity Rarity => rarity;
    }
}

