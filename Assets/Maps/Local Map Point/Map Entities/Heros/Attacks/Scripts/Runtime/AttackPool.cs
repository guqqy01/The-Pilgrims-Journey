using UnityEngine;
using UnityEngine.Pool;

namespace Heros.Attacks
{
    public class AttackPool
    {
        private readonly ObjectPool<AttackSpawnBase> pool;
        private readonly AttackSpawnBase prefab;
        private readonly Transform parent;

        public AttackPool(AttackSpawnBase prefab, Transform parent = null, int defaultCapacity = 10, int maxSize = 100)
        {
            this.prefab = prefab;
            this.parent = parent;

            pool = new ObjectPool<AttackSpawnBase>(
                Create,
                OnGet,
                OnRelease,
                OnDestroy,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }

        private AttackSpawnBase Create()
        {
            AttackSpawnBase a = Object.Instantiate(prefab, parent);
            a.gameObject.SetActive(false);
            return a;
        }

        private void OnGet(AttackSpawnBase a)
        {
            if (parent != null)
                a.transform.SetParent(parent);

            a.gameObject.SetActive(true);
            a.SetReturnToPoolCallback(() => Release(a));
            a.OnPoolGet();
        }

        private void OnRelease(AttackSpawnBase a)
        {
            a.OnPoolRelease();
            a.SetReturnToPoolCallback(null);
            a.gameObject.SetActive(false);
            if (parent != null)
                a.transform.SetParent(parent);
        }

        private void OnDestroy(AttackSpawnBase a)
        {
            Object.Destroy(a.gameObject);
        }

        public AttackSpawnBase Get(Vector3 position, Quaternion rotation)
        {
            AttackSpawnBase a = pool.Get();
            a.transform.position = position;
            a.transform.rotation = rotation;
            return a;
        }

        public void Release(AttackSpawnBase a)
        {
            pool.Release(a);
        }
    }
}
