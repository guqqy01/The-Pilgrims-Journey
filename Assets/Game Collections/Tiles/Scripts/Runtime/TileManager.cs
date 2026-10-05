using UnityEngine;
using System.Collections.Generic;
using Tiles;
namespace TPJ.Gameplay
{
    public class TileManager : Singleton<TileManager>
    {
        [SerializeField]
        private TileManagerSettings settings;

        public List<TileSO> AllTiles => settings.AllTiles;
        public List<TileSO> HeroTiles => settings.HeroTiles;
        public List<TileSO> UnitTiles => settings.UnitTiles;
        public List<TileSO> MonsterTiles => settings.MonsterTiles;
    }
}
