using Monsters;
namespace Tiles
{
    public class NormalMonsterTile : MonsterTile<NormalMonsterSpawn>
    {
        public NormalMonsterTile(TileSO tileSO, NormalMonsterSpawn monsterSpawn) : base(tileSO, monsterSpawn) { }
    }
}

