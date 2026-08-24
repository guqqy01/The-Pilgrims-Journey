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
                if(_object == null)
                {
                    Debug.LogWarning("_monsterSpawn is null");
                    return null;
                }

                if(_object is not T genericMonsterSpawn)
                {
                    Debug.LogError($"{nameof(MonsterSpawn)} is not the required Type: {typeof(T)}");
                    return null;
                }

                return genericMonsterSpawn;
            }
        }

        protected MonsterTile(TileSO tileSO, T monsterSpawn) : base(monsterSpawn, tileSO){}
    }
}

