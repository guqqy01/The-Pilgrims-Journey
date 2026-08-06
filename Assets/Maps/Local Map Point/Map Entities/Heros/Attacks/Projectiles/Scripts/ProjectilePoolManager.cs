using System.Collections.Generic;
using UnityEngine;

namespace Heros.Attacks.Projectiles
{
    public static class ProjectilePoolManager
    {
        private static readonly Dictionary<int, ProjectilePool> pools = new Dictionary<int, ProjectilePool>();

        public static ProjectilePool GetPool(Projectile prefab, Transform parent = null)
        {
            if (prefab == null)
                return null;

            int id = prefab.GetInstanceID();
            if (!pools.TryGetValue(id, out var pool))
            {
                pool = new ProjectilePool(prefab, parent);
                pools[id] = pool;
            }

            return pool;
        }
    }
}
