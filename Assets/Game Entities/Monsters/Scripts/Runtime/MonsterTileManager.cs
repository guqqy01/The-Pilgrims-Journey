using Monsters;
using System.Collections.Generic;
using UnityEngine;
namespace Tiles
{

    public class MonsterTileManager : Singleton<MonsterTileManager>
    {
        [SerializeField]
        private WeightedChanceList<TileSO> tiles;

        public MonsterTileBase GetMonsterTile(TileSO tile, MonsterSpawn monster)
        {
            if (monster is NormalMonsterSpawn normal)
                return new NormalMonsterTile(tile, normal);

            if (monster is WaveMonsterSpawn wave)
                return new WaveMonsterTile(tile, wave);

            return null;
        }

        public List<MonsterTileBase> GetMonsterTiles(MonsterSpawnSettings monsterSpawnSettings, List<TileSO> tiles, int amountOfTiles)
        {
            List<MonsterTileBase> returnMonsterTiles = new List<MonsterTileBase>();

            List<NormalMonsterSpawn> normalMonsterSpawns = monsterSpawnSettings.normalMonsterSpawns;
            List<WaveMonsterSpawn> waveMonsterSpawns = monsterSpawnSettings.waveMonsterSpawns;

            List<MonsterSpawn> monsterSpawns = new List<MonsterSpawn>();
            monsterSpawns.AddRange(normalMonsterSpawns);
            monsterSpawns.AddRange(waveMonsterSpawns);

            for (int i = 0; i < amountOfTiles; i++)
            {
                MonsterSpawn monsterSpawn = monsterSpawns.GetRandom();
                MonsterTileBase monsterTile = GetMonsterTile(tiles.GetRandom(), monsterSpawn);

                if (monsterTile == null)
                    continue;

                returnMonsterTiles.Add(monsterTile);
            }

            return returnMonsterTiles;
        }
    }

}
