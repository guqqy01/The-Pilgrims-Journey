using Monsters;
namespace Tiles
{
    public abstract class MonsterTileBase : TileBase
    {
        protected MonsterSpawn _monsterSpawn;

        public MonsterSpawn MonsterSpawnBase { get { return _monsterSpawn; } }

        protected MonsterTileBase(TileSO tileSO, MonsterSpawn monsterSpawn) : base(tileSO)
        {
            _monsterSpawn = monsterSpawn;
        }

        public float Difficulty
        {
            get
            {
                return Shape.Count * _monsterSpawn.Difficulty; 
            }
        }

    }
}

