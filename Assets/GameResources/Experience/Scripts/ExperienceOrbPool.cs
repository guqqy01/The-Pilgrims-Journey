using UnityEngine;
using UnityEngine.Pool;
namespace LevelingSystem
{
    public class ExperienceOrbPool
    {
        private readonly ObjectPool<ExperienceOrb> pool;
        private readonly ExperienceOrb prefab;
        private readonly int experienceValue;
        private readonly Transform parent;

        public ExperienceOrbPool(
            ExperienceOrb prefab,
            int experienceValue,
            Transform parent,
            int defaultCapacity = 20,
            int maxSize = 200)
        {
            this.prefab = prefab;
            this.experienceValue = experienceValue;
            this.parent = parent;

            pool = new ObjectPool<ExperienceOrb>(
                Create,
                OnGet,
                OnRelease,
                OnDestroy,
                collectionCheck: false,
                defaultCapacity,
                maxSize);
        }

        private ExperienceOrb Create()
        {
            ExperienceOrb orb = Object.Instantiate(prefab, parent);
            orb.gameObject.SetActive(false);
            return orb;
        }

        private void OnGet(ExperienceOrb orb)
        {
            orb.Initialize(experienceValue, this);

            if (orb.TryGetComponent(out Collider2D col))
                col.enabled = true;
        }

        private void OnRelease(ExperienceOrb orb)
        {
            if (orb.TryGetComponent(out Collider2D col))
                col.enabled = false;

            orb.gameObject.SetActive(false);
            orb.transform.SetParent(parent);
        }

        private void OnDestroy(ExperienceOrb orb)
        {
            Object.Destroy(orb.gameObject);
        }

        public ExperienceOrb Get(Vector3 position)
        {
            ExperienceOrb orb = pool.Get();
            orb.transform.position = position;
            return orb;
        }

        public void Release(ExperienceOrb orb)
        {
            pool.Release(orb);
        }
    }

}