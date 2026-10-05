using UnityEngine;
using System.Collections.Generic;
using Tiles;

namespace TPJ.Gameplay
{
    [CreateAssetMenu(menuName = "Tiles/Tile Manager Settings")]
    public class TileManagerSettings : ScriptableObject
    {
        [SerializeField]
        private List<TileSO> allTiles;
        public List<TileSO> AllTiles => allTiles;
        [SerializeField]
        private List<TileSO> heroTiles;
        public List<TileSO> HeroTiles => heroTiles;
        [SerializeField]
        private List<TileSO> unitTiles;
        public List<TileSO> UnitTiles => unitTiles;
        [SerializeField]
        private List<TileSO> monsterTiles;
        public List<TileSO> MonsterTiles => monsterTiles;
    }
}
