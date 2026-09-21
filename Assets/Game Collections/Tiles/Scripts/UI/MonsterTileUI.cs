using Monsters;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
namespace Tile.UI
{
    public class MonsterTileUI : TileUI<MonsterTileBase>, IDescriptable
    {
        public MonsterSpawn MonsterSpawn
        {
            get
            {
                if (ObjectTile == null)
                    return null;

                return ObjectTile.MonsterSpawnBase;
            }
        }

        public string Title => MonsterSpawn.monster.Title;

        public string MainDescription => MonsterSpawn.monster.MainDescription;

        public Sprite Sprite => MonsterSpawn.monster.Sprite;

        public List<ISubDescriptionData> GetSubDescriptionData => MonsterSpawn.monster.GetSubDescriptionData;

        public string ShortDescription => MonsterSpawn.monster.ShortDescription;
    }

}
