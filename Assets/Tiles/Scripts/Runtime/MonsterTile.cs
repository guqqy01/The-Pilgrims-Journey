using Monsters;
using UnityEngine;
namespace Tiles
{
    public abstract class MonsterTile<T> : MonsterTileBase where T : MonsterSpawn
    {
        public T MonsterSpawn
        {
            get
            {
                if(_monsterSpawn == null)
                {
                    Debug.LogWarning("_monsterSpawn is null");
                    return null;
                }

                if(_monsterSpawn is not T genericMonsterSpawn)
                {
                    Debug.LogError($"{nameof(_monsterSpawn)} is not the required Type: {typeof(T)}");
                    return null;
                }

                return genericMonsterSpawn;
            }
        }

        protected MonsterTile(TileSO tileSO, T monsterSpawn) : base(tileSO, monsterSpawn){}
    }
}

