using System.Collections.Generic;
using UnityEngine;

namespace Attacks
{
    public static class AttackPoolManager
    {
        private static readonly Dictionary<int, AttackPool> pools = new Dictionary<int, AttackPool>();

        public static AttackPool GetPool(AttackSpawnBase prefab, Transform parent = null)
        {
            if (prefab == null)
                return null;

            int id = prefab.GetInstanceID();
            if (!pools.TryGetValue(id, out var pool))
            {
                pool = new AttackPool(prefab, parent);
                pools[id] = pool;
            }

            return pool;
        }
    }
}
