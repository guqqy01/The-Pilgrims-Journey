using System.Collections.Generic;
using UnityEngine;

namespace Heros.Attacks
{
    public static class AttackSpawnPoolManager
    {
        private static readonly Dictionary<int, AttackSpawnPool> pools = new Dictionary<int, AttackSpawnPool>();

        public static AttackSpawnPool GetPool(AttackSpawnBase prefab, Transform parent = null)
        {
            if (prefab == null)
                return null;

            int id = prefab.GetInstanceID();
            if (!pools.TryGetValue(id, out var pool))
            {
                pool = new AttackSpawnPool(prefab, parent);
                pools[id] = pool;
            }

            return pool;
        }
    }
}
