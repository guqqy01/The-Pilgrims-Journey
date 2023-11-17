using UnityEngine;

namespace Map
{
    [CreateAssetMenu]
    public class MonsterStats : ScriptableObject
    {
        public int Map;

        public int baseMapPoints;

        public int mapPointsPerTileInRange;
    }
}


