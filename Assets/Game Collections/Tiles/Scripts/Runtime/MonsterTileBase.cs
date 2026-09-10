using Monsters;
using System;
namespace Tiles
{
    [Serializable]
    public abstract class MonsterTileBase : Tile<MonsterSpawn>
    {
        public MonsterSpawn MonsterSpawnBase => TiledObject as MonsterSpawn;

        protected MonsterTileBase(MonsterSpawn obj, TileSO tileSO) : base(obj, tileSO) { }

        public float Difficulty
        {
            get
            {
                return Shape.Count * MonsterSpawnBase.Difficulty; 
            }
        }

        
    }
}

