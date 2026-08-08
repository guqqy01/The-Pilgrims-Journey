using UnityEngine;
using UnityEngine.Pool;

namespace Attacks.Projectiles
{
    public class ProjectilePool
    {
        private readonly ObjectPool<Projectile> pool;
        private readonly Projectile prefab;
        private readonly Transform parent;

        public ProjectilePool(Projectile prefab, Transform parent = null, int defaultCapacity = 20, int maxSize = 200)
        {
            this.prefab = prefab;
            this.parent = parent;

            pool = new ObjectPool<Projectile>(
                Create,
                OnGet,
                OnRelease,
                OnDestroy,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }

        private Projectile Create()
        {
            Projectile p = Object.Instantiate(prefab, parent);
            p.gameObject.SetActive(false);
            return p;
        }

        private void OnGet(Projectile p)
        {
            if (p == null)
                return;

            if (parent != null)
                p.transform.SetParent(parent);

            p.gameObject.SetActive(true);
            // set the callback so the projectile can return itself to this pool
            p.SetReturnToPoolCallback(() => Release(p));
            p.OnPoolGet();
        }

        private void OnRelease(Projectile p)
        {
            p.OnPoolRelease();
            p.SetReturnToPoolCallback(null);
            p.gameObject.SetActive(false);
            if (parent != null)
                p.transform.SetParent(parent);
        }

        private void OnDestroy(Projectile p)
        {
            Object.Destroy(p.gameObject);
        }

        public Projectile Get(Vector3 position, Quaternion rotation)
        {
            Projectile p = pool.Get();

            // Defensive: Unity can surface a destroyed object through the pool if it was destroyed elsewhere.
            // If the returned object is destroyed ("null" in Unity terms), create a fresh instance and initialize it
            // like a pooled item to ensure we never return a destroyed reference.
            if (p == null)
            {
                p = Create();
                OnGet(p);
            }

            p.transform.position = position;
            p.transform.rotation = rotation;
            return p;
        }

        public void Release(Projectile p)
        {
            pool.Release(p);
        }
    }
}
