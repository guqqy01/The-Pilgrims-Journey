using System.Collections.Generic;
using UnityEngine;

namespace Attacks
{
    public static class AttackSpawnPoolManager
    {
        private static readonly Dictionary<EntityId, AttackSpawnPool> pools = new Dictionary<EntityId, AttackSpawnPool>();

        public static AttackSpawnPool GetPool(AttackSpawnBase prefab, Transform parent = null)
        {
            if (prefab == null)
                return null;

            EntityId entityId = prefab.GetEntityId();
            if (!pools.TryGetValue(entityId, out var pool))
            {
                pool = new AttackSpawnPool(prefab, parent);
                pools[entityId] = pool;
            }

            return pool;
        }
    }
}
