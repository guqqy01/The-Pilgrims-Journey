using Monsters;
using UnityEngine;
namespace Tiles
{
    public class MonsterTile : TileBase
    {
        private MonsterSpawn _monsterSpawn;
        public MonsterSpawn MonsterSpawn { get { return _monsterSpawn; } }
        public MonsterTile(TileSO tileSO, MonsterSpawn monsterSpawn) : base(tileSO)
        {
            _monsterSpawn = monsterSpawn;
        }

        public float Difficulty
        {
            get
            {
                return Shape.Count * MonsterSpawn.Difficulty; 
            }
        }

    }
}

